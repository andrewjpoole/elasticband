using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public class ElasticBand : IElasticBand
    {
        private string _elasticsearchUri = "http://localhost:9200";
        private string _username;
        private string _password;
        private string _apiKey;
        private JsonSerializerOptions _jsonSerializerOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IElasticQueryBuilder _queryBuilder;

        public ElasticBand(IHttpClientFactory httpClientFactory, IElasticQueryBuilder queryBuilder)
        {            
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                     
            _httpClientFactory = httpClientFactory;
            _queryBuilder = queryBuilder;
        }

        /// <summary>
        /// Override the default Elasticsearch URL
        /// </summary>
        /// <param name="url">A string containing the url</param>
        public void SetElasticsearchUrl(string url) 
        {
            _elasticsearchUri = url;
        }

        /// <summary>
        /// Method to set username and password, used to dd a basic auth header to authenticate with elasticsearch.
        /// </summary>
        /// <param name="username">A string containing the username.</param>
        /// <param name="password">A string containing the password.</param>
        public void SetElasticsearchAuthentication(string username, string password) 
        {
            if (string.IsNullOrEmpty(username + password))
                throw new ArgumentException("username and password must not be empty");

            _username = username;
            _password = password;
        }

        /// <summary>
        /// Method to set apiKey, used to dd a basic auth header to authenticate with elasticsearch.
        /// </summary>
        /// <param name="apiKey">A string containing the preconfigured apiKey.</param>
        public void SetElasticsearchAuthentication(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("apiKey must not be empty");

            _apiKey = apiKey;
        }

        /// <summary>
        /// Override the Json serializer options
        /// </summary>
        /// <param name="options">A JsonSerializerOptions containing specified options, to be applied to all serialisation calls.</param>
        public void SetJsonSerialiserOptions(JsonSerializerOptions options) 
        {
            _jsonSerializerOptions = options;
        }

        /// <summary>
        /// Fetch an object (T) from Elasticsearch, fast lookup by id.
        /// </summary>
        /// <typeparam name="T">The Type of the object to query.</typeparam>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="id">A string containing the id of the object to fetch.</param>
        /// <returns>An ElasticBandResponse, where Data is an object (T) containing the fetched object.</returns>
        public async Task<ElasticBandResponse<T>> GetDocumentByID<T>(string index, string id)
        {
            CheckIndex(index);

            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id must not be null or empty");

            var requestUri = $"{index}/_doc/{id}";
            using (var client = GetClient())
            {
                var response = await client.GetAsync(requestUri);
                var warnings = GetWarnings(response.Headers);

                var json = await response.Content.ReadAsStringAsync();
                var errors = CheckJsonForErrors(json);
                var data = (ElasticDocumentResponse<T>)JsonSerializer.Deserialize(json, typeof(ElasticDocumentResponse<T>), _jsonSerializerOptions);

                if (response.IsSuccessStatusCode)
                {
                    return new ElasticBandResponse<T>()
                    {
                        Ok = true,
                        StatusCode = response.StatusCode,
                        Result = data.found ? "found" : "not_found",
                        Warnings = warnings,
                        Errors = errors,
                        DataJson = json,
                        Data = data._source,
                        Id = id
                    };
                }

                return new ElasticBandResponse<T>()
                {
                    Ok = false,
                    Warnings = warnings,
                    Errors = errors,
                    DataJson = json,
                    StatusCode = response.StatusCode,
                    Result = data.found ? "found" : "not_found",
                    Id = id
                };
            }
        }

        /// <summary>
        /// Fetch objects (T) using a query.
        /// </summary>
        /// <typeparam name="T">The Type of the object to query.</typeparam>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="searchTerms">The query string, see documentation for examples</param>
        /// <param name="useQueryBuilder">A bool which determines if the query string should be used to generate ES query syntax to be used in the body of the request to Elasticsearch. Defaults to true.</param>
        /// <param name="limit">An int which if using the QueryBuilder, will liit the number of objects returned. Defaults to 500.</param>
        /// <returns>An ElasticBandResponse, where Data is a List<T> containing the matching objects.</returns>
        public async Task<ElasticBandResponse<List<T>>> Query<T>(string index, string searchTerms, bool useQueryBuilder = true, int limit = 500)
        {
            CheckIndex(index);

            var requestUri = $"{index}/_search";
            using (var client = GetClient())
            {
                HttpResponseMessage response = null;


                string query;

                if (useQueryBuilder)
                    query = _queryBuilder.Build(searchTerms, limit);
                else
                    query = searchTerms;

                if (string.IsNullOrEmpty(query))
                {
                    response = await client.GetAsync(requestUri);
                }
                else
                {
                    var content = new StringContent(query, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(requestUri, content);
                }

                var warnings = GetWarnings(response.Headers);

                var json = await response.Content.ReadAsStringAsync();
                var errors = CheckJsonForErrors(json);

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<ElasticQueryResponse<T>>(json, _jsonSerializerOptions);
                    var hits = data.hits.hits.Select(h => h._source).ToList();
                    var aggregationBuckets = data.aggregations?
                        .ToDictionary(agg => agg.Key, agg => agg.Value.buckets
                            .Select(bkt => new ElasticBucket
                            {
                                Key = bkt.key,
                                Count = bkt.doc_count
                            }).ToList());

                    return new ElasticBandResponse<List<T>>()
                    {
                        Ok = true,
                        StatusCode = response.StatusCode,
                        Warnings = warnings,
                        Errors = errors,
                        DataJson = json,
                        Data = hits,
                        Result = hits.Any() ? "found" : "not_found",
                        AggregationBuckets = aggregationBuckets
                    };
                }

                return new ElasticBandResponse<List<T>>()
                {
                    Ok = false,
                    Warnings = warnings,
                    Errors = errors,
                    StatusCode = response.StatusCode,
                    DataJson = json,
                    Result = "not_found"
                };
            }
        }

        /// <summary>
        /// Index an object (T) into Elasticsearch (create or update).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="data">The object to index.</param>
        /// <param name="id">A string containing the id of the object to index. If empty, a new uid will be generated.</param>
        /// <returns>An ElasticBandResponse</returns>
        public async Task<ElasticBandResponse<T>> Index<T>(string index, T data, string id = null)
        {
            CheckIndex(index);

            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            var requestUri = $"{index}/_doc/{id}";
            using (var client = GetClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(data, _jsonSerializerOptions), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, content);
                var warnings = GetWarnings(response.Headers);

                var json = await response.Content.ReadAsStringAsync();
                var errors = CheckJsonForErrors(json);
                var responseData = (ElasticIndexResult)JsonSerializer.Deserialize(json, typeof(ElasticIndexResult), _jsonSerializerOptions);
                
                if (response.IsSuccessStatusCode)
                {                    
                    return new ElasticBandResponse<T>()
                    {
                        Ok = true,
                        StatusCode = response.StatusCode,
                        Result = responseData.result,
                        Id = id,
                        Warnings = warnings,
                        Errors = errors,
                        DataJson = json,
                        Data = data,
                    };
                }

                return new ElasticBandResponse<T>()
                {
                    Ok = false,
                    Warnings = warnings,
                    Errors = errors,
                    StatusCode = response.StatusCode,
                    Result = responseData.result,
                    DataJson = json
                };
            }
        }

        /// <summary>
        /// Delete an object from an index.
        /// </summary>
        /// <param name="index">A string containing the Elasticsearch index name.</param>
        /// <param name="id">A string containing the id of the object to fetch.</param>
        /// <returns>An ElasticBandResponse</returns>
        public async Task<ElasticBandResponse> Delete(string index, string id)
        {
            CheckIndex(index);

            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id must not be null or empty");

            var requestUri = $"{index}/_doc/{id}";
            using (var client = GetClient())
            {
                var response = await client.DeleteAsync(requestUri);
                var warnings = GetWarnings(response.Headers);

                var json = await response.Content.ReadAsStringAsync();
                var errors = CheckJsonForErrors(json);
                var responseData = (ElasticIndexResult)JsonSerializer.Deserialize(json, typeof(ElasticIndexResult), _jsonSerializerOptions);

                return new ElasticBandResponse
                {
                    Ok = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Result = responseData?.result,
                    DataJson = json,
                    Warnings = warnings,
                    Errors = errors,
                    Id = id
                };
            }
        }
        
        private string GetWarnings(HttpResponseHeaders headers)
        {
            var warnings = new StringBuilder();
            if (headers.Contains("Warning"))
            {
                foreach (var warning in headers.Warning)
                {
                    warnings.AppendLine(warning.ToString());
                }
            }
            return warnings.ToString();
        }

        private string CheckJsonForErrors(string json) 
        {
            var errorResponse = JsonSerializer.Deserialize<ElasticErrorResponse>(json);
            if (errorResponse.error != null)
            {
                return errorResponse.error.reason;
            }
            return string.Empty;
        }

        private void CheckIndex(string index)
        {
            if (string.IsNullOrEmpty(index))
                throw new ArgumentException("Elasticsearch Index name must not be empty.");
        }

        /// <summary>
        /// Access a client configured to send requests to Elasticsearch
        /// </summary>
        /// <returns>An HttpClient</returns>
        public HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_elasticsearchUri);

            client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrEmpty(_apiKey))
            {
                string creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(_apiKey));
                client.DefaultRequestHeaders.Add("Authorization", "ApiKey " + creds);
            }
            else if (!string.IsNullOrEmpty(_username + _password)) 
            {
                string creds = Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + _password));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + creds);            
            }

            return client;

            
        }        
    }
}
