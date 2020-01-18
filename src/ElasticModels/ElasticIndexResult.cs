#pragma warning disable IDE1006 // Naming Styles
namespace AJP.ElasticBand
{
    public class ElasticIndexResult
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public int _version { get; set; }
        public string result { get; set; }
        public int _seq_no { get; set; }
        public int _primary_term { get; set; }
    }


}
