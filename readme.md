# ElasticBand

## WHAT?

A wrapper around the plain http rest api, i.e. the one the the kibana dev tools uses.

Strongly typed, but no dependencies. i.e. does not depend on NEST or newtonsoft.json.

It basically only uses only httpClient and the new faster serialisers in System.Text.Json

It covers basic CRUD operations and supports queries either via rest body (i.e. like kibana dev tools) or simple text queries like 'name:andrew* and notes:*banana*'.

## LIMITATIONS?

I haven't included calls to the bulk api, the official client libraries should be used instead.

## CHANGES

Feel free to change/contribute