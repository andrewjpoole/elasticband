#pragma warning disable IDE1006 // Naming Styles
namespace AJP.ElasticBand
{
    public class ElasticQueryResponse<T>
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public Hits<T> hits { get; set; }
        public Aggregations aggregations { get; set; }
    }
}