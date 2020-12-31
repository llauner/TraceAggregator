using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceAggregator.Dto;
using TraceAggregator.Services.Interfaces;

namespace TraceAggregator.Services
{
    public class AggregatorService : IAggregatorService
    {
        private readonly ILogger _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IZipStorageService _zipStorageService;
        private readonly IStorageService _storageService;

        private static readonly string YearlyTracksFilename = $"{DateTime.Now.Year}-tracks.geojson";
        private static readonly string YearlyTracksZipFilename = $"{DateTime.Now.Year}-tracks.geojson.zip";

        public const short DefaultCoordinatesReductionFactor = 50;

        public AggregatorService(ILogger<AggregatorService> logger, 
                                IConfigurationService configurationService,
                                IZipStorageService zipStorageService,
                                IStorageService storageService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _zipStorageService = zipStorageService;
            _storageService = storageService;
        }

        public static string StreamToString(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Run
        /// Default Aggregation entry point: process backlog and integrated reduced daily files into yearly cumulative tracks file.
        /// </summary>
        /// <param name="keepBacklog">Set to true to preserve backlog (files will not be deleted once processed)</param>
        /// <returns></returns>
        public async Task Run(float? reductionFactor = AggregatorService.DefaultCoordinatesReductionFactor,
                                bool keepBacklog=false)
        {
            _logger.LogInformation("######### [AggregatorService] Building yearly cumulative track ... ###########");
            var appliedReductionFactor = reductionFactor.HasValue ? reductionFactor.Value : AggregatorService.DefaultCoordinatesReductionFactor;

            // --- List files to be processed in backlog ---
            var backlogFilesList = _storageService.GetFilenameList("backlog");
            backlogFilesList = backlogFilesList.Where(f => Path.GetExtension(f) == ".zip").ToList();     // Keep zip files only

            if (backlogFilesList.Count > 0)
            {
                // --- Get yearly geojson
                _logger.LogInformation($"Downloading zip file from bucket: {_configurationService.TraceAggregatorBucketName} / {YearlyTracksZipFilename}");
                GeoJsonDto yearGeojson = null;
                try
                {
                    var yearlyGeojsonFile = await _zipStorageService.DownloadZipedFileAsStringAsync(YearlyTracksZipFilename);
                    yearGeojson = JsonConvert.DeserializeObject<GeoJsonDto>(yearlyGeojsonFile);
                }
                catch (Google.GoogleApiException e)
                {
                    _logger.LogWarning($"[AggregatorService]: {e.Message}");
                    yearGeojson = new GeoJsonFeatureCollectionDto();
                }
               

                // --- Process files ---
                var currentFileCount = 0;
                var totalFileCount = backlogFilesList.Count;

                foreach (var filename in backlogFilesList)
                {
                    // --- Get daily geojson
                    _logger.LogInformation($"{currentFileCount}/{totalFileCount} Integrating file into yearly tracks: {filename}");
                    currentFileCount++;
                    
                    var tracksForDayAsJson = await _zipStorageService.DownloadZipedFileAsStringAsync(filename);
                    var dayGeojson = JsonConvert.DeserializeObject<GeoJsonDto>(tracksForDayAsJson);

                    //--- Reduce number of features
                    ReduceGeojsonFeatures(ref dayGeojson, appliedReductionFactor);
                    yearGeojson.features.AddRange(dayGeojson.features);     // Add the features to the yearly aggregation

                    // --- Delete the processed file from the backlog
                    if (!keepBacklog)
                    {
                        await _storageService.DeleteFileAsync(filename);
                    }
                }

                // --- Store yearly geojson file with new feature added
                _logger.LogInformation($"Storing new yearly tracemap into bucket ...");
                var yearlyGeojsonText = JsonConvert.SerializeObject(yearGeojson);
                await _zipStorageService.UploadStringToZipFileAsync(yearlyGeojsonText, YearlyTracksFilename, YearlyTracksZipFilename);   // Store into a GCP bucket: geojson

                _logger.LogInformation("######### [AggregatorService] ######### Done !");

            }
            else
            {
                _logger.LogInformation("######### [AggregatorService] ######### No file to process in the backlog. Done !");
            }
        }


        /// <summary>
        /// ReduceCumulativeTracksZipFile
        /// Reduce the features in an existing .geojson.zip file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="reductionFactor"></param>
        /// <returns></returns>
        public async Task ReduceCumulativeTracksZipFile(string filename= null, 
                                                        float? reductionFactor = AggregatorService.DefaultCoordinatesReductionFactor, 
                                                        bool doBackup=true)
        {
            filename= string.IsNullOrEmpty(filename)? YearlyTracksZipFilename : filename;
            var appliedReductionFactor = reductionFactor.HasValue ? reductionFactor.Value : AggregatorService.DefaultCoordinatesReductionFactor;

            _logger.LogInformation($"######### [AggregatorService] ######### Reducing .geojson.zip file: {filename} / {reductionFactor}");

            // --- Get file as Geojson ---
            var targetGeojsonFile = await _zipStorageService.DownloadZipedFileAsStringAsync(filename);
            var targetGeojson = JsonConvert.DeserializeObject<GeoJsonDto>(targetGeojsonFile);

            // Make a backup if needed
            if (doBackup)
            {
                var backupFilename = $"{DateTime.Now.ToString("yyyy_MM_dd")}#{filename}";
                var backupEntryName = filename.Replace(".zip", "");
                await _zipStorageService.UploadStringToZipFileAsync(targetGeojsonFile, backupEntryName, backupFilename);
                _logger.LogInformation($"Bakcup of file made: {filename} -> {backupFilename}");
            }

            // --- Reduce ----
            ReduceGeojsonFeatures(ref targetGeojson, appliedReductionFactor);

            // --- Write result ---
            _logger.LogInformation($"Storing reduced file into bucket ...");
            var geojsonText = JsonConvert.SerializeObject(targetGeojson);
            await _zipStorageService.UploadStringToZipFileAsync(geojsonText, filename.Replace(".zip",""), filename);    // Store into a GCP bucket: geojson

            _logger.LogInformation("######### [AggregatorService] ######### Done !");
        }


        /// <summary>
        /// ReduceGeojsonFeatures
        /// </summary>
        /// <param name="sourceGeojson"></param>
        /// <param name="reductionFactor"></param>
        private void ReduceGeojsonFeatures(ref GeoJsonDto sourceGeojson, float reductionFactor= AggregatorService.DefaultCoordinatesReductionFactor)
        {
            //--- Process daily file: reduce the number of features
            var totalInitialCoordinatesCount = 0;
            var totalReducedCoordinatesCount = 0;
            foreach (var f in sourceGeojson.features)
            {
                // Reduce the number of coordinates per feature
                totalInitialCoordinatesCount += f.geometry.coordinates.Count;
                var reducedCoordiantes = f.geometry.coordinates.Where((_, i) => i % reductionFactor == 0).ToList();
                f.geometry.coordinates = reducedCoordiantes;
                totalReducedCoordinatesCount += reducedCoordiantes.Count;
            }
            _logger.LogInformation($"Reduced Coordinates: {totalReducedCoordinatesCount} = {totalInitialCoordinatesCount} / {reductionFactor}");
        }


    }


   
}
