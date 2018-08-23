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
const config = require('./config.json');
const uuidv4 = require('uuid/v4');
const NodeRSA = require('node-rsa');

// Imports the Google Cloud client library
const Logging = require('@google-cloud/logging');

// Your Google Cloud Platform project ID
const projectId = config.GCLOUD_PROJECT;

// Creates a client
const logging = new Logging({
  projectId: projectId,
});

const logName = 'main-app';
const log = logging.log(logName);
const logResource = {
  type: 'global',
};

const app = express();

const rsaVerifier = new NodeRSA(config.FREIEWAHL_PUBLICKEY_CHALLENGE_PEM);

app.use(bodyParser.json()); // support json encoded bodies
app.use(bodyParser.urlencoded({ extended: true })); // support encoded bodies

const TOKEN_COUNT = config.TOKEN_COUNT;

app.get('/', (req, res) => {
  console.log('received request, sending response');
  var result = '<html><head><title>Hello</title></head><body><form action="/foo" method="post"><input type="submit" value="foo" /></form>Version 5</body></html>';
  res.status(200).send(result).end();
});

app.get('/registerTokens', (req, res) => {
  console.log('received request, sending response');
  var result = '<html><head><title>Hello</title></head><body><form action="/registerTokens" method="post"><input type="hidden" name="tokens"' + ' value=\'[{ "index": 1, "tokenId": "t1234", "blindingFactor": "b123456" }, { "index": 2, "tokenId": "t4321", "blindingFactor": "b654321" }]\'/>' + '<input type="hidden" name="registrationId" value="1231231232"/><input type="hidden" name="votingId" value="1231231232"/><input type="hidden" name="email" value="michfasch@gmx.at"/><input type="submit" value="foo" /></form>';
  res.status(200).send(result).end();
});

app.post('/grantRegistration', async function(req, res) {
  var registrationId = req.body.registrationId;
  var challenge = await dbwrapper.getChallenge(registrationId);

  const challengeSignature = Buffer.from(req.body.challengeSignature, 'base64');
  const challengeBuffer = Buffer.from(challenge);
  var result = rsaVerifier.verify(challengeBuffer, challengeSignature);
  if(result) {
    log.entry({resource: logResource}, 'Successfully received signature for challenge for registration ' + registrationId);
  }
  else {
    log.entry({resource: logResource}, 'Received invalid signature for challenge for registration ' + registrationId);
  }

  console.log(req.body.tokens);
  res.status(200).send("OK!").end;
});

app.post('/getChallengeAndTokens', async function(req, res) {
  var challenge = uuidv4();
  var date = Date.now();
  var tokens = await dbwrapper.setChallengeAndGetTokens(req.body.registrationId, challenge, date.toString());

  res.json({"challenge": challenge, "tokens": tokens}).end;
});

app.post('/saveRegistrationDetails', (req, res) => {
  var registrationId = req.body.id;
  var email = req.body.mail;
  var password = req.body.password;
  var i;
  var tokens = [];
  var blindingFactors = [];
  for(i = 0; i < TOKEN_COUNT; i++) {
    var token = tokengenerator.generateToken();
    var blindedToken = tokengenerator.blindToken(token, password);
    blindingFactors[i] = blindedToken.r;
    tokens[i] = blindedToken.blinded;
  }
  log.entry({resource: logResource}, 'Saving Registration details for registration with id' + registrationId);
  dbwrapper.registerTokens(registrationId, email, tokens, blindingFactors)
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
