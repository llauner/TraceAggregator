using System.Collections.Generic;


namespace TraceAggregator.Dto
{
    public class GeoJsonFeatureCollectionDto : GeoJsonDto
    {
        public GeoJsonFeatureCollectionDto(): base()
        {
            this.features = new List<Feature>();
        }

    }
}
