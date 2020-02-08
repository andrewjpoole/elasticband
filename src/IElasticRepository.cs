using System.Collections.Generic;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public interface IElasticRepository<T>
    {
        /// <summary>
        /// The index name in Elasticsearch.
        /// </summary>
        string IndexName { get; }

        /// <summary>
        /// Delete an object.
        /// </summary>
        /// <param name="id">A string containing the id of the object to delete.</param>
        /// <returns>An ElasticBandResponse</returns>
        Task<ElasticBandResponse> Delete(string id);

        /// <summary>
        /// Fetch an object (T) from Elasticsearch, fast lookup by id.
        /// </summary>
        /// <param name="id">A string containing the id of the document in Elasticsearch</param>
        /// <returns>An ElasticBandResponse, where Data is an object (T) containing the fetched object.</returns>
        Task<ElasticBandResponse<T>> GetById(string id);

        /// <summary>
        /// Index an object(T) into Elasticsearch (create or update).
        /// </summary>
        /// <param name="id">A string containing the id of the document, if empty a new Guid will be generated.</param>
        /// <param name="data">The object to index.</param>
        /// <returns>An ElasticBandResponse</returns>
        Task<ElasticBandResponse<T>> Index(string id, T data);

        /// <summary>
        /// Fetch objects (T) using a query
        /// </summary>
        /// <param name="query">The query string, see documentation for examples</param>
        /// <param name="useQueryBuilder">A bool which determines if the query string should be used to generate ES query syntax to be used in the body of the request to Elasticsearch. Defaults to true.</param>
        /// <param name="limit">An int which if using the QueryBuilder, will liit the number of objects returned. Defaults to 500.</param>
        /// <returns>An ElasticBandResponse, where Data is a List<T> containing the matching objects.</returns>
        Task<ElasticBandResponse<List<T>>> Query(string query, bool useQueryBuilder = true, int limit = 500);

        IElasticBand GetElasticBand();
    }
}
