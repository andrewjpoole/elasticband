# ElasticBand
 
 ![build](https://andrewjpoole.visualstudio.com/elasticband/_apis/build/status/andrewjpoole.elasticband?branchName=master)
 
## What is it?

A wrapper around the plain http rest api, i.e. the one the the kibana dev tools uses.

Strongly typed, but no dependencies. i.e. does not depend on NEST or newtonsoft.json.

It basically only uses only httpClient and the new faster serialisers in System.Text.Json

It covers basic CRUD operations and supports queries either via rest body (i.e. like kibana dev tools) or simple text queries like 'name:andrew* and notes:*banana*'.

## Limitations?

It doesn't include any calls to the bulk api as yet, the official client libraries should be used instead for that.

## CHANGES

Feel free to change/contribute

## Rest API

I have also created a rest api over the top of the ElasticBand, making it trivial to use Elasticsearch as a backend for storing simple collections of items via Swagger, which dynamically renders a controller for each collection etc see project [here](https://github.com/andrewjpoole/elasticband.api)
