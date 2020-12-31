using System.Threading.Tasks;

namespace TraceAggregator.Services
{
    public interface IZipStorageService
    {
        Task<string> DownloadZipedFileAsStringAsync(string filename);
        Task UploadStringToZipFileAsync(string textToUpload, string zipEntryName, string zipFilename);
    }
}