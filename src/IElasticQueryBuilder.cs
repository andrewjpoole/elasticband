namespace AJP.ElasticBand
{
    public interface IElasticQueryBuilder 
    {
        string Build(string searchString, int limit = 500);
    }
}
