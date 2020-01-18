using System.Collections.Generic;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public abstract class ElasticRepository<T> : IElasticRepository<T>
    {
        private readonly IElasticBand _elasticBand;

        public string IndexName { get; }

        public ElasticRepository(string indexName, IElasticBand elasticBand)
        {            
            _elasticBand = elasticBand;
            IndexName = indexName;
        }

        public async Task<ElasticBandResponse<T>> Index(string id, T data)
        {
            return await _elasticBand.Index(IndexName, data, id);
        }

        public async Task<ElasticBandResponse<T>> GetById(string id)
        {
            return await _elasticBand.GetDocumentByID<T>(IndexName, id);
        }

        public async Task<ElasticBandResponse<List<T>>> Query(string query)
        {
            return await _elasticBand.Query<T>(IndexName, query);
        }

        public async Task<ElasticBandResponse> Delete(string id)
        {
            return await _elasticBand.Delete(IndexName, id);
        }

        public IElasticBand GetElasticBand()
        {
            return _elasticBand;
        }
    }
}
