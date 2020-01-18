using System.Collections.Generic;
using System.Threading.Tasks;

namespace AJP.ElasticBand
{
    public interface IElasticRepository<T>
    {
        string IndexName { get; }

        Task<ElasticBandResponse> Delete(string id);
        Task<ElasticBandResponse<T>> GetById(string id);
        Task<ElasticBandResponse<T>> Index(string id, T data);
        Task<ElasticBandResponse<List<T>>> Query(string query);

        IElasticBand GetElasticBand();
    }
}
