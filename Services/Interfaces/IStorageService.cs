using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TraceAggregator.Services.Interfaces
{
    public interface IStorageService
    {
        IList<string> GetFilenameList(string prefix);
        Task<MemoryStream> DownloadObjectFromBucketAsync(string objectName);
        Task UploadToBucketAsync(string objectName, Stream inStream);
        Task DeleteFileAsync(string filename);
    }
}