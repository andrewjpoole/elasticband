using System.Net.Http;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;   //Is valid
                }

                if (cert.GetCertHashString().ToLower() == "0b9c2ad6fb339c96da57c879b047b7d4281e6a5f")
                {
                    return true;
                }
                return false;
            };

            return new HttpClient(httpClientHandler);
        }
    }    
}