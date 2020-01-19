using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AJP.ElasticBand;

namespace Tests
{
    [TestFixture]
    [Category("Integration")]
    public class ElasticBandIntegrationTests
    {
        private ElasticBand _sut;
        private readonly string _index = "test_index";

        [SetUp]
        public void Setup()
        {
            _sut = new ElasticBand(new ElasticQueryBuilder());
            _sut.GetClient().DeleteAsync(_index).Wait();
        }

        [TearDown]
        public void Teardown()
        {
            _sut = new ElasticBand(new ElasticQueryBuilder());
            _sut.GetClient().DeleteAsync(_index).Wait();
        }

        [Test]
        public async Task ElasticBand_methods_should_index_get_query_and_delete_documents_in_elasticsearch()
        {
            var newId1 = Guid.NewGuid().ToString();
            var doc1Response = await _sut.Index(_index, new TestObject { Name = "aaa", ebDataType = "TestObject" }, newId1);
            Assert.That(doc1Response.Result, Is.EqualTo("created"));

            var newId2 = Guid.NewGuid().ToString();
            var doc2Response = await _sut.Index(_index, new TestObject { Name = "bbb", ebDataType = "TestObject" }, newId2);
            Assert.That(doc2Response.Result, Is.EqualTo("created"));

            var doc1 = await _sut.GetDocumentByID<TestObject>(_index, newId1);
            Assert.That(doc1.Id, Is.EqualTo(newId1));
            Assert.That(doc1.Data.Name, Is.EqualTo("aaa"));

            var doc2 = await _sut.GetDocumentByID<TestObject>(_index, newId2);
            Assert.That(doc2.Id, Is.EqualTo(newId2));
            Assert.That(doc2.Data.Name, Is.EqualTo("bbb"));
                       
            doc1.Data.Name = "ccc";
            var doc1EditedResponse = await _sut.Index(_index, doc1.Data, doc1.Id);
            Assert.That(doc1EditedResponse.Id, Is.EqualTo(newId1));
            Assert.That(doc1EditedResponse.Data.Name, Is.EqualTo("ccc"));

            await Task.Delay(1000); // Give ES a chance to internally update the indicies

            var query = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestData\\es_query2.txt");

            var queryResponse = await _sut.Query<TestObject>(_index, query);
            Assert.That(queryResponse.Result, Is.EqualTo("found"));
            Assert.That(queryResponse.Data.Count, Is.EqualTo(2));
            Assert.That(queryResponse.Data.First().Name, Is.EqualTo("bbb"));

            var deleteResponse1 = await _sut.Delete(_index, newId1);
            Assert.That(deleteResponse1.Result, Is.EqualTo("deleted"));

            var deleteResponse2 = await _sut.Delete(_index, newId2);
            Assert.That(deleteResponse2.Result, Is.EqualTo("deleted"));

            await Task.Delay(1000); // Give ES a chance to internally update the indicies

            var finalQueryResponse = await _sut.Query<TestObject>(_index, query);
            Assert.That(finalQueryResponse.Result, Is.EqualTo("not_found"));
            Assert.That(finalQueryResponse.Data.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ElasticBand_methods_should_handle_bucket_aggregations_in_elasticsearch()
        {
            var newId1 = Guid.NewGuid().ToString();
            var doc1Response = await _sut.Index(_index, new TestObject { Name = "aaa", ebDataType = "TestObject" }, newId1);
            Assert.That(doc1Response.Result, Is.EqualTo("created"));

            var newId2 = Guid.NewGuid().ToString();
            var doc2Response = await _sut.Index(_index, new TestObject { Name = "aaa", ebDataType = "TestObject" }, newId2);
            Assert.That(doc2Response.Result, Is.EqualTo("created"));

            var newId3 = Guid.NewGuid().ToString();
            var doc3Response = await _sut.Index(_index, new TestObject { Name = "bbb", ebDataType = "TestObject" }, newId3);
            Assert.That(doc3Response.Result, Is.EqualTo("created"));

            var doc1 = await _sut.GetDocumentByID<TestObject>(_index, newId1);
            Assert.That(doc1.Id, Is.EqualTo(newId1));
            Assert.That(doc1.Data.Name, Is.EqualTo("aaa"));

            var doc2 = await _sut.GetDocumentByID<TestObject>(_index, newId2);
            Assert.That(doc2.Id, Is.EqualTo(newId2));
            Assert.That(doc2.Data.Name, Is.EqualTo("aaa"));

            var doc3 = await _sut.GetDocumentByID<TestObject>(_index, newId3);
            Assert.That(doc3.Id, Is.EqualTo(newId3));
            Assert.That(doc3.Data.Name, Is.EqualTo("bbb"));

            await Task.Delay(1000); // Give ES a chance to internally update the indicies

            var query = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestData\\es_agg_query.txt.txt");

            var queryResponse = await _sut.Query<TestObject>(_index, query);
            Assert.That(queryResponse.Result, Is.EqualTo("found"));
            Assert.That(queryResponse.Data.Count, Is.EqualTo(3));
            Assert.That(queryResponse.Data.First().Name, Is.EqualTo("aaa"));

            // Test that aggregations come back correctly
            Assert.That(queryResponse.AggregationBuckets.ContainsKey("test_aggregation"));
            Assert.That(queryResponse.AggregationBuckets["test_aggregation"].Count, Is.EqualTo(2));
            Assert.That(queryResponse.AggregationBuckets["test_aggregation"][0].Key, Is.EqualTo("aaa"));
            Assert.That(queryResponse.AggregationBuckets["test_aggregation"][0].Count, Is.EqualTo(2));

            var deleteResponse1 = await _sut.Delete(_index, newId1);
            Assert.That(deleteResponse1.Result, Is.EqualTo("deleted"));

            var deleteResponse2 = await _sut.Delete(_index, newId2);
            Assert.That(deleteResponse2.Result, Is.EqualTo("deleted"));

            var deleteResponse3 = await _sut.Delete(_index, newId3);
            Assert.That(deleteResponse3.Result, Is.EqualTo("deleted"));

            await Task.Delay(1000); // Give ES a chance to internally update the indicies

            var finalQueryResponse = await _sut.Query<TestObject>(_index, query);
            Assert.That(finalQueryResponse.Result, Is.EqualTo("not_found"));
            Assert.That(finalQueryResponse.Data.Count, Is.EqualTo(0));
        }
    }
}