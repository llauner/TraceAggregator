using System.Threading.Tasks;

namespace TraceAggregator.Services.Interfaces
{
    public interface IAggregatorService
    {
       Task Run(float? reductionFactor = AggregatorService.DefaultCoordinatesReductionFactor, bool keepBacklog=false);
        Task ReduceCumulativeTracksZipFile(string filename=null, float? reductionFactor= AggregatorService.DefaultCoordinatesReductionFactor, bool doBackup=true);
    }
}