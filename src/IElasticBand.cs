using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public interface IElasticBand
    {
        /// <summary>
        /// Override the default Elasticsearch URL
        /// </summary>
        /// <param name="url">A string containing the url</param>
        void SetElasticsearchUrl(string url);

        void SetElasticsearchAuthentication(string apiKey);

        void SetElasticsearchAuthentication(string username, string password);

        /// <summary>
        /// Override the Json serializer options
        /// </summary>
        /// <param name="options">A JsonSerializerOptions containing specified options, to be applied to all serialisation calls.</param>
        void SetJsonSerialiserOptions(JsonSerializerOptions options);

        /// <summary>
        /// Delete an object from an index.
        /// </summary>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="id">A string containing the id of the object to fetch.</param>
        /// <returns>An ElasticBandResponse</returns>
        Task<ElasticBandResponse> Delete(string index, string id);


        Task<ElasticBandResponse<T>> GetDocumentByID<T>(string index, string id);

        /// <summary>
        /// Index an object (T) into Elasticsearch (create or update).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="data">The object to index.</param>
        /// <param name="id">A string containing the id of the object to index. If empty, a new uid will be generated.</param>
        /// <returns>An ElasticBandResponse</returns>
        Task<ElasticBandResponse<T>> Index<T>(string index, T data, string id = null);

        /// <summary>
        /// Fetch objects (T) using a query.
        /// </summary>
        /// <typeparam name="T">The Type of the object to query.</typeparam>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="searchTerms">The query string, see documentation for examples</param>
        /// <param name="useQueryBuilder">A bool which determines if the query string should be used to generate ES query syntax to be used in the body of the request to Elasticsearch. Defaults to true.</param>
        /// <param name="limit">An int which if using the QueryBuilder, will liit the number of objects returned. Defaults to 500.</param>
        /// <returns>An ElasticBandResponse, where Data is a List<T> containing the matching objects.</returns>

        Task<ElasticBandResponse<List<T>>> Query<T>(string index, string query, bool useQueryBuilder = true, int limit = 500);

        /// <summary>
        /// Access a client configured to send requests to Elasticsearch
        /// </summary>
        /// <returns>An HttpClient</returns>
        HttpClient GetClient();
    }
}