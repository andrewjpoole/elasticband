using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public class ElasticBand : IElasticBand
    {
        private string _elasticsearchUri = "http://localhost:9200";
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

        public void SetElasticsearchUrl(string url) 
        {
            _elasticsearchUri = url;
        }

        public void SetJsonSerialiserOptions(JsonSerializerOptions options) 
        {
            _jsonSerializerOptions = options;
        }

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
                var data = (ElasticDocumentResponse<T>)JsonSerializer.Deserialize(json, typeof(ElasticDocumentResponse<T>), _jsonSerializerOptions);

                if (response.IsSuccessStatusCode)
                {
                    return new ElasticBandResponse<T>()
                    {
                        Ok = true,
                        StatusCode = response.StatusCode,
                        Result = data.found ? "found" : "not_found",
                        Warnings = warnings,
                        DataJson = json,
                        Data = data._source,
                        Id = id
                    };
                }

                return new ElasticBandResponse<T>()
                {
                    Ok = false,
                    Warnings = warnings,
                    StatusCode = response.StatusCode,
                    Result = data.found ? "found" : "not_found",
                    Id = id
                };
            }
        }

        public async Task<ElasticBandResponse<List<T>>> Query<T>(string index, string searchTerms, bool useQueryBuilder = true)
        {
            CheckIndex(index);

            var requestUri = $"{index}/_search";
            using (var client = GetClient())
            {
                HttpResponseMessage response = null;


                string query;

                if (useQueryBuilder)
                    query = _queryBuilder.Build(searchTerms);
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
                    StatusCode = response.StatusCode,
                    DataJson = json,
                    Result = "not_found"
                };
            }
        }
        
        public async Task<ElasticBandResponse<T>> Index<T>(string index, T data, string id = null)
        {
            CheckIndex(index);

            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id must not be null or empty");

            var requestUri = $"{index}/_doc/{id}";
            using (var client = GetClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(data, _jsonSerializerOptions), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, content);
                var warnings = GetWarnings(response.Headers);

                var json = await response.Content.ReadAsStringAsync();
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
                        DataJson = json,
                        Data = data,
                    };
                }

                return new ElasticBandResponse<T>()
                {
                    Ok = false,
                    Warnings = warnings,
                    StatusCode = response.StatusCode,
                    Result = responseData.result,
                    DataJson = json
                };
            }
        }

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
                var responseData = (ElasticIndexResult)JsonSerializer.Deserialize(json, typeof(ElasticIndexResult), _jsonSerializerOptions);

                return new ElasticBandResponse
                {
                    Ok = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Result = responseData?.result,
                    DataJson = json,
                    Warnings = warnings,
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

        private void CheckIndex(string index)
        {
            if (string.IsNullOrEmpty(index))
                throw new ArgumentException("Elasticsearch Index name must not be empty.");
        }

        public HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_elasticsearchUri);

            client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }        
    }
}
