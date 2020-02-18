using NUnit.Framework;
using System;
using System.IO;
using AJP.ElasticBand;

namespace Tests
{
    [TestFixture]
    public class QueryBuilderTests 
    {
        [TestCase("", "")]
        [TestCase("email:gmail* and notes:bass*", "expected_query1.txt")]
        [TestCase("email:gmail*", "expected_query2.txt")]
        [TestCase("bassguitar", "expected_query3.txt")]
        [TestCase("birthday < 2019-02-07T22:16:52.626Z", "expected_query4.txt")]
        [TestCase("name > h", "expected_query5.txt")]
        [TestCase("emailAddress:a@b.com", "expected_query6.txt")]
        public void QueryBuilder_should_return_correct_query_for_string(string searchString, string expectedQueryFileName) 
        {
            var sut = new ElasticQueryBuilder();
            var result = sut.Build(searchString);

            Console.WriteLine(result);
            
            var expected = GetTestData(expectedQueryFileName);

            // remove spaces and carriage return newline chars to make comparison easier...
            result = result.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            expected = expected.Replace("\r", "").Replace("\n", "").Replace(" ", "");

            Assert.That(result, Is.EqualTo(expected));
        }

        private string GetTestData(string fileName) 
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var path =  Path.Combine(TestContext.CurrentContext.WorkDirectory, $"TestData\\{fileName}");
            return File.ReadAllText(path);
        }
    }
}