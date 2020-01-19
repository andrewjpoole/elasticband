using NUnit.Framework;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using AJP.ElasticBand;

namespace Tests
{
    [TestFixture]
    [Category("Integration")]
    public class ElasticRepositoryIntegrationTests
    {
        private ElasticBand _elasticBand;
        private readonly string _index = "test_index";

        [SetUp]
        public void Setup()
        {
            _elasticBand = new ElasticBand(new DefaultHttpClientFactory(), new ElasticQueryBuilder());
            _elasticBand.GetClient().DeleteAsync(_index).Wait();
        }

        [TearDown]
        public void Teardown()
        {
            _elasticBand = new ElasticBand(new DefaultHttpClientFactory(), new ElasticQueryBuilder());
            _elasticBand.GetClient().DeleteAsync(_index).Wait();
        }

        [Test]
        public async Task ElasticRepository_methods_should_index_get_query_and_delete_documents_in_elasticsearch()
        {
            var sut = new ElasticObjectRepository<object>(_elasticBand);

            var item = new
            {
                Name = "Bob",
                Category = "Person",
                Age = 75,
                CreatedDate = DateTime.Now
            };

            var newId = Guid.NewGuid().ToString();

            var response1 = await sut.Index(newId, item);
            Assert.That(response1.Result, Is.EqualTo("created"));

            var fetchedItemResponse = await sut.GetById(newId);
            Assert.That(fetchedItemResponse.Result, Is.EqualTo("found"));

            var fetchedItem = (JsonElement)fetchedItemResponse.Data;
            Assert.That(fetchedItem.GetProperty("category").ToString(), Is.EqualTo("Person"));           

        }
    }

    public class ElasticObjectRepository<T> : ElasticRepository<T>
    {
        public ElasticObjectRepository(IElasticBand elasticBand) : base("test_index", elasticBand)
        {
        }
    }
}