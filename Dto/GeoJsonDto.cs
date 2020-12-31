using System.Collections.Generic;

namespace TraceAggregator.Dto
{
    public class Geometry
    {
        public string type { get; set; }
        public List<List<double>> coordinates { get; set; }
    }

    public class Properties
    {
    }

    public class Feature
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class GeoJsonDto
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }
}
