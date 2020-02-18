# ElasticBand
 
 ![build](https://andrewjpoole.visualstudio.com/elasticband/_apis/build/status/andrewjpoole.elasticband?branchName=master)
 
## What is it?

A wrapper around the plain http rest api, i.e. the one the the kibana dev tools uses.

Strongly typed, but no dependencies. i.e. does not depend on NEST or newtonsoft.json.

It basically only uses only httpClient and the new faster serialisers in System.Text.Json

It covers basic CRUD operations and supports queries either via rest body (i.e. like kibana dev tools) or simple text queries like 'name:andrew* and notes:*banana*'.

## How to use it?

Either use the IElasticBand directly or the IElasticRepository to abstract away the index name:

### ElasticBand Directly

```c#
// Given the class
public class TestObject2 
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

[Test]
public async Task ElasticBand_methods_should_index_get_query_and_delete_documents_in_elasticsearch_for_type_TestObject2() 
{
    // Decide the index name and instantiate an ElasticBand...
    var index = "testindex2";
    var elasticBand = new ElasticBand(new DefaultHttpClientFactory(), new ElasticQueryBuilder());
    elasticBand.GetClient().DeleteAsync(index).Wait();

    // Create some objects to index
    var object1 = new TestObject2 { Name = "Andrew", Birthday = new DateTime(2019, 2, 7) };
    var object2 = new TestObject2 { Name = "James", Birthday = new DateTime(2019, 7, 27) };
    var object3 = new TestObject2 { Name = "Drew", Birthday = new DateTime(2019, 1, 9) };

    // Index the objects, optionally pass in an id, if omitted a new Guid will be generated...
    var indexResponse1 = await elasticBand.Index<TestObject2>(index, object1, "id1");
    var indexResponse2 = await elasticBand.Index<TestObject2>(index, object2, "id2");
    var indexResponse3 = await elasticBand.Index<TestObject2>(index, object3, "id3");

    // Speedy lookup if you know an id...
    var getResponse1 = await elasticBand.GetDocumentByID<TestObject2>(index, "id2");

    // Update a property and re-index...
    getResponse1.Data.Name = "Jamie";
    await elasticBand.Index<TestObject2>(index, getResponse1.Data, "id2");

    await Task.Delay(1000); // Give ES a chance to internally update the indicies

    // Query in various ways...
    var queryResult1 = await elasticBand.Query<TestObject2>(index, "Jamie");
    var queryResult2 = await elasticBand.Query<TestObject2>(index, "name:Drew");
    var queryResult3 = await elasticBand.Query<TestObject2>(index, "birthday>2019-03-01");
    var queryResult4 = await elasticBand.Query<TestObject2>(index, "birthday<2019-03-01T09:05:00");
    var queryResult5 = await elasticBand.Query<TestObject2>(index, "*rew");

    // Delete...
    await elasticBand.Delete(index, "id2");

    Assert.That(queryResult1.Data.Count, Is.EqualTo(1));
    Assert.That(queryResult2.Data.Count, Is.EqualTo(1));
    Assert.That(queryResult3.Data.Count, Is.EqualTo(1));
    Assert.That(queryResult4.Data.Count, Is.EqualTo(2));
    Assert.That(queryResult5.Data.Count, Is.EqualTo(2));
}

```

### ElasticRepository

```c#
[Test]
public async Task ElasticRepository_methods_should_index_get_query_and_delete_documents_in_elasticsearch()
{
    // This test uses a type of object, which is similar to JObject/dynamic

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
```

## Limitations?

It doesn't include any calls to the bulk api as yet, the official client libraries should be used instead for that.

Elasticsearch auth will be coming soon.

## License

Feel free to use for whatever - MIT license. If its usefull please let me know!

## Contributing

Contributions would be most welcome! the only thing I ask is that new features are covered by minimal unit tests, along the same lines as the existing ones. Please create a PR.

## Rest API

I have also created a rest api over the top of the ElasticBand, making it trivial to use Elasticsearch as a backend for storing simple collections of items via Swagger, which dynamically renders a controller for each collection etc see project [here](https://github.com/andrewjpoole/elasticband.api)
