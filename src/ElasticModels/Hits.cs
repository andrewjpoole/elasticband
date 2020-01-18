#pragma warning disable IDE1006 // Naming Styles
using System.Collections.Generic;

namespace AJP.ElasticBand
{
    public class Hits<T>
    {
        public List<Hit<T>> hits { get; set; }
    }
}
