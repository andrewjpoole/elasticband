#pragma warning disable IDE1006 // Naming Styles
namespace AJP.ElasticBand
{
    public class Hit<T>
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public T _source { get; set; }
    }
}
