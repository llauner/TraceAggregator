using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TraceAggregator.Services.Interfaces;

namespace TraceAggregator.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILogger _logger;
        private readonly IConfigurationService _configuration;
        private readonly StorageClient _storageClient;

        public StorageService(ILoggerFactory loggerFactory, IConfigurationService configuration)
        {
            _logger = loggerFactory.CreateLogger<StorageService>();
            _configuration = configuration;
            _storageClient = StorageClient.Create();
        }

        /// <summary>
        /// GetFilenameList
        /// </summary>
        /// <returns></returns>
        public IList<string> GetFilenameList(string prefix)
        {
            var enumerable = _storageClient.ListObjects(_configuration.TraceAggregatorBucketName, prefix);
            var list = enumerable.ToList();
            var filenameList = list.Select(o => o.Name).ToList();

            return filenameList;
        }

        /// <summary>
        /// UploadToBucket
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="inStream"></param>
        public async Task UploadToBucketAsync(string objectName, Stream inStream)
        {
            await _storageClient.UploadObjectAsync(_configuration.TraceAggregatorBucketName, objectName, "text/plain", inStream);
        }

        public async Task<MemoryStream> DownloadObjectFromBucketAsync(string objectName)
        {
            var memoryStream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_configuration.TraceAggregatorBucketName, objectName, memoryStream);

            return memoryStream;
        }


        /// <summary>
        /// DeleteFileAsync
        /// </summary>
        /// <param name="filename">The full file name to delete</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string filename)
        {
            await _storageClient.DeleteObjectAsync(_configuration.TraceAggregatorBucketName, filename);
        }




    }
}
