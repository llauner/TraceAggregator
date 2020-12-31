namespace TraceAggregator.Services.Interfaces
{
    public interface IConfigurationService
    {
        
        string GcpProjectId { get; }
        string TraceAggregatorBucketName { get; }
    }
}
