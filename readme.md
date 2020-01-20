# ElasticBand
 
 [![Build Status](https://andrewjpoole.visualstudio.com/elasticband/_apis/build/status/andrewjpoole.elasticband?branchName=master)](https://andrewjpoole.visualstudio.com/elasticband/_build/latest?definitionId=3&branchName=master)

## WHAT?

A wrapper around the plain http rest api, i.e. the one the the kibana dev tools uses.

Strongly typed, but no dependencies. i.e. does not depend on NEST or newtonsoft.json.

It basically only uses only httpClient and the new faster serialisers in System.Text.Json

It covers basic CRUD operations and supports queries either via rest body (i.e. like kibana dev tools) or simple text queries like 'name:andrew* and notes:*banana*'.

## LIMITATIONS?

I haven't included calls to the bulk api, the official client libraries should be used instead.

## CHANGES

Feel free to change/contribute

## Rest API

I have also created a rest api over the top of the ElasticBand, making it trivial to use Elasticsearch as a backend for storing simple collections of items via Swagger, which dynamically renders a controller for each collection etc see project [here](https://github.com/andrewjpoole/elasticband.api)
