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
const dbwrapper = require('./dbwrapper')

const app = express();

app.get('/', (req, res) => {
  console.log('received request, sending response');
  var result = '<html><head><title>Hello</title></head><body><form action="/foo" method="post"><input type="submit" value="foo" /></form>';
  res.status(200).send(result).end();
});

app.get('/add', (req, res) => {
  console.log('received add request, sending response');
  dbwrapper.sayHelloInGerman()
    .then((count) => {
      res.status(200).send('Hello, world! ' + count).end();
  });
});

app.post('/foo', (req, res) => {
  console.log('posted for foo');
  res.status(200).send('hello foo').end();
})

// Start the server
const PORT = process.env.PORT || 8082;
app.listen(PORT, () => {
  console.log(`App listening on port ${PORT}`);
  console.log('Press Ctrl+C to quit.');
});
// [END app]
