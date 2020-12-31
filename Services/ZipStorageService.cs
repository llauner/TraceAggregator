using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TraceAggregator.Extension;

namespace TraceAggregator.Services.Interfaces
{
    public class ZipStorageService : IZipStorageService
    {
        private readonly IStorageService _storageService;

        public ZipStorageService(IStorageService storageService)
        {
            _storageService = storageService;
        }


        /// <summary>
        /// DownloadZipedFileAsString
        /// Download .zip file from bucket, unzip and return as string
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The string inside the first entry of the zip archive</returns>
        public async Task<string> DownloadZipedFileAsStringAsync(string filename)
        {
            string fileAsString = null;
            using (var fileStream = await _storageService.DownloadObjectFromBucketAsync(filename))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                using (var zipArchive = new ZipArchive(fileStream))
                {
                    var firestEntry = zipArchive.Entries[0];
                    var unzippedStream = firestEntry.Open();

                    fileAsString = unzippedStream.ReadAsString();
                }
            }
            return fileAsString;
        }

        /// <summary>
        /// UploadStringToZipFileAsync
        /// </summary>
        /// <param name="textToUpload"></param>
        /// <param name="zipEntryName"></param>
        /// <param name="zipFilename"></param>
        public async Task UploadStringToZipFileAsync(string textToUpload, string zipEntryName, string zipFilename)
        {
            var yearlyGeojsonTextStream = textToUpload.ToStream();
            // Upload .geojson file
            // HACK: Commented out as this is generating big files
            //_storageService.UploadToBucket(YearlyTracksFilename, yearlyGeojsonTextStream);        // Upload .geojson file

            // Store into a GCP bucket: zip
            yearlyGeojsonTextStream.Seek(0, SeekOrigin.Begin);
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipEntry = archive.CreateEntry(zipEntryName);
                    using (var entryStream = zipEntry.Open())
                    {
                        yearlyGeojsonTextStream.CopyTo(entryStream);
                    }
                }
                await _storageService.UploadToBucketAsync(zipFilename, memoryStream);
            }
            yearlyGeojsonTextStream.Dispose();
        }


    }
}
