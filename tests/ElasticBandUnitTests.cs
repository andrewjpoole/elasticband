using System.Net.Http;
using System.Text;
using NUnit.Framework;
using AJP.ElasticBand;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class ElasticBandUnitTests 
    {
        private IHttpClientFactory _fakeHttpClientFactory;
        private FakeHttpMessageHandler _fakeHttpMessageHandler;
        private HttpClient _httpClient; 

        [Test]
        public void Index_should_return_an_ElasticBandResponse_given_the_expected_http_response_string() 
        {
            var content = "{\"_index\":\"test_index\",\"_type\":\"_doc\",\"_id\":\"5c506881-37bb-4525-ba8c-09980e74a056\",\"_version\":1,\"result\":\"created\",\"_shards\":{\"total\":2,\"successful\":1,\"failed\":0},\"_seq_no\":0,\"_primary_term\":1}";
            ConfigureClientFactory(content);

            var sut = new ElasticBand(_fakeHttpClientFactory, new ElasticQueryBuilder());

            var testObject = new TestObject { Name = "aaa", ebDataType = "TestObject" };

            var result = sut.Index("testIndex", testObject, "testId").Result;

            Assert.That(result.Ok, Is.EqualTo(true));
            Assert.That(result.Result, Is.EqualTo("created"));
            Assert.That(result.Id, Is.EqualTo("testId"));
        }

        [Test]
        public void Delete_should_return_an_ElasticBandResponse_given_the_expected_http_response_string()
        {
            var content = "{\"_index\":\"test_index\",\"_type\":\"_doc\",\"_id\":\"ae3f81be-fa22-4777-984d-17855357b156\",\"_version\":3,\"result\":\"deleted\",\"_shards\":{\"total\":2,\"successful\":1,\"failed\":0},\"_seq_no\":3,\"_primary_term\":1}";
            ConfigureClientFactory(content);

            var sut = new ElasticBand(_fakeHttpClientFactory, new ElasticQueryBuilder());

            var result = sut.Delete("testIndex", "testId").Result;

            Assert.That(result.Ok, Is.EqualTo(true));
            Assert.That(result.Result, Is.EqualTo("deleted"));
            Assert.That(result.Id, Is.EqualTo("testId"));
        }

        [Test]
        public void GetDocumentById_should_return_an_ElasticBandResponse_given_the_expected_http_response_string()
        {
            var content = "{\"_index\":\"test_index\",\"_type\":\"_doc\",\"_id\":\"cb2b1bed-fea4-41b4-b526-8eb607205583\",\"_version\":1,\"_seq_no\":0,\"_primary_term\":1,\"found\":true,\"_source\":{\r\n  \"name\": \"aaa\",\r\n  \"ebDataType\": \"TestObject\",\r\n  \"created\": \"2020-01-19T20:09:48.1433274+00:00\"\r\n}}";
            ConfigureClientFactory(content);

            var sut = new ElasticBand(_fakeHttpClientFactory, new ElasticQueryBuilder());

            var testObject = new TestObject { Name = "aaa", ebDataType = "TestObject" };

            var result = sut.GetDocumentByID<TestObject>("testIndex", "testId").Result;

            Assert.That(result.Ok, Is.EqualTo(true));
            Assert.That(result.Result, Is.EqualTo("found"));
            Assert.That(result.Id, Is.EqualTo("testId"));
            Assert.That(result.Data.ebDataType, Is.EqualTo(testObject.ebDataType));
        }
        
        [Test]
        public void Query_should_return_an_ElasticBandResponse_given_the_expected_http_response_string()
        {
            var content = "{\"took\":1,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":{\"value\":2,\"relation\":\"eq\"},\"max_score\":0.13353139,\"hits\":[{\"_index\":\"test_index\",\"_type\":\"_doc\",\"_id\":\"196f168d-37ef-4160-982d-afe15d7670eb\",\"_version\":1,\"_score\":0.13353139,\"_source\":{\r\n  \"name\": \"bbb\",\r\n  \"ebDataType\": \"TestObject\",\r\n  \"created\": \"2020-01-19T20:33:12.5661565+00:00\"\r\n}},{\"_index\":\"test_index\",\"_type\":\"_doc\",\"_id\":\"3dc0de1e-a187-488d-84b4-80f2323cfed9\",\"_version\":2,\"_score\":0.13353139,\"_source\":{\r\n  \"name\": \"ccc\",\r\n  \"ebDataType\": \"TestObject\",\r\n  \"created\": \"2020-01-19T20:33:12.065047+00:00\"\r\n}}]}}";
            ConfigureClientFactory(content);

            var sut = new ElasticBand(_fakeHttpClientFactory, new ElasticQueryBuilder());

            var testObject = new TestObject { Name = "aaa", ebDataType = "TestObject" };

            var result = sut.Query<TestObject>("testIndex", "").Result;

            Assert.That(result.Ok, Is.EqualTo(true));
            Assert.That(result.Result, Is.EqualTo("found"));
            Assert.That(result.Data.FirstOrDefault().ebDataType, Is.EqualTo(testObject.ebDataType));
        }


        public void ConfigureClientFactory(string content) 
        {
            _fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
            _httpClient = new HttpClient(_fakeHttpMessageHandler);
            _fakeHttpClientFactory = new FakeIHttpClientFactory(_httpClient);            
        }
    }
}