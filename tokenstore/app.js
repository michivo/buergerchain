/**
 * Copyright 2017, Google, Inc.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

'use strict';

// [START app]
const express = require('express');
const dbwrapper = require('./dbwrapper');
const tokengenerator = require('./tokengenerator');
const bodyParser = require('body-parser');

const app = express();

app.use(bodyParser.json()); // support json encoded bodies
app.use(bodyParser.urlencoded({ extended: true })); // support encoded bodies

app.get('/', (req, res) => {
  console.log('received request, sending response');
  var result = '<html><head><title>Hello</title></head><body><form action="/foo" method="post"><input type="submit" value="foo" /></form>';
  res.status(200).send(result).end();
});

app.get('/registerTokens', (req, res) => {
  console.log('received request, sending response');
  var result = '<html><head><title>Hello</title></head><body><form action="/registerTokens" method="post"><input type="hidden" name="tokens"' + ' value=\'[{ "index": 1, "tokenId": "t1234", "blindingFactor": "b123456" }, { "index": 2, "tokenId": "t4321", "blindingFactor": "b654321" }]\'/>' + '<input type="hidden" name="registrationId" value="1231231232"/><input type="hidden" name="votingId" value="1231231232"/><input type="hidden" name="email" value="michfasch@gmx.at"/><input type="submit" value="foo" /></form>';
  res.status(200).send(result).end();
});

app.post('/registerTokens', (req, res) => {
  var registrationId = req.body.registrationId;
  var votingId = req.body.votingId;
  var email = req.body.email;
  var tokens = JSON.parse(req.body.tokens);

  dbwrapper.registerTokens(registrationId, votingId, email, tokens)
    .then(() => {
      res.status(200).send("OK!").end;
  });
});

// Start the server
const PORT = process.env.PORT || 8082;
app.listen(PORT, () => {
  console.log(`App listening on port ${PORT}`);
  console.log('Press Ctrl+C to quit.');
});
// [END app]
