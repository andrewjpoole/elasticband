﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public interface IElasticBand
    {
        Task<ElasticBandResponse> Delete(string index, string id);
        Task<ElasticBandResponse<T>> GetDocumentByID<T>(string index, string id);
        Task<ElasticBandResponse<T>> Index<T>(string index, T data, string id = null);
        Task<ElasticBandResponse<List<T>>> Query<T>(string index, string query, bool useQueryBuilder = true);
        HttpClient GetClient();
    }
}