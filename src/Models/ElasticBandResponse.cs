using System.Collections.Generic;
using System.Net;


namespace AJP.ElasticBand
{
    public class ElasticBandResponse
    {
        public bool Ok { get; set; }
        public string DataJson { get; set; }
        public string Id { get; set; }
        public string Warnings { get; set; }
        public string Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class ElasticBandResponse<T> : ElasticBandResponse
    {
        public T Data { get; set; }
        public Dictionary<string, List<ElasticBucket>> AggregationBuckets { get; set; }
    }
}
