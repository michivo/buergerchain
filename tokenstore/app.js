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
const mailProvider = require('./mailprovider.js');
const admin = require('firebase-admin');
const sha256 = require('sha256');

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

const serviceAccount = require('./credentials.json');

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: 'https://freiewahl-application.firebaseio.com'
});

app.use(bodyParser.json({ limit: '5mb' })); // support json encoded bodies
app.use(bodyParser.urlencoded({ extended: true })); // support encoded bodies
app.use(allowCrossDomain);

const MAX_TOKEN_COUNT = config.MAX_TOKEN_COUNT;

app.get('/', (req, res) => {
  const result = '<html><head><title>Hello</title></head><body><form action="/foo" method="post"><input type="submit" value="foo" /></form>Version 7</body></html>';
  res.status(200).send(result).end();
});

app.post('/grantRegistration', wrapAsync(async function (req, res) {
  const registrationId = req.body.registrationId;
  const challenge = await dbwrapper.getChallenge(registrationId);

  const challengeSignature = Buffer.from(req.body.challengeSignature, 'base64');
  const challengeBuffer = Buffer.from(challenge);
  const result = rsaVerifier.verify(challengeBuffer, challengeSignature);
  if (result) {
    //console.log('Successfully received signature for challenge for registration ' + registrationId);
    log.entry({ resource: logResource }, 'Successfully received signature for challenge for registration ' + registrationId);
  }
  else {
    // TODO: error!
    // console.log('Received invalid signature for challenge for registration ' + registrationId);
    log.entry({ resource: logResource }, 'Received invalid signature for challenge for registration ' + registrationId);
  }

  const registration = await dbwrapper.getRegistration(registrationId);
  const voterId = uuidv4();
  const votingId = req.body.votingId;
  const votingTitle = req.body.votingTitle;
  const startDate = req.body.startDate;
  const endDate = req.body.endDate;
  const link = req.body.link + `?votingId=${votingId}&voterId=${voterId}`;
  // console.log('inserting registration with id ' + registrationId + ', voter id ' + voterId + ', voting id ' + votingId);
  await dbwrapper.insertVotingTokens(votingId, voterId, registration.tokens, req.body.tokens, registration.blindingFactors);
  await dbwrapper.updatePasswordHash(registrationId, voterId);
  await dbwrapper.deleteRegistration(registrationId);
  mailProvider.sendInvitation(registration.email, votingTitle, startDate, endDate, link);

  res.status(200).send('OK!').end;
}));

app.post('/getChallengeAndTokens', wrapAsync(async function (req, res) {
  const challenge = uuidv4();
  const date = Date.now();
  const tokens = await dbwrapper.setChallengeAndGetTokens(req.body.registrationId, challenge, date.toString());

  res.json({ 'challenge': challenge, 'tokens': tokens }).end;
}));

app.post('/setKeys', wrapAsync(async function (req, res) {
  const exponents = req.body.exponents;
  const moduli = req.body.moduli;
  const votingId = req.body.votingId;
  await dbwrapper.insertKeys(votingId, exponents, moduli);
  res.status(200).send('OK!').end;
}));

app.post('/getTokens', wrapAsync(async function (req, res) {
  const voterId = req.body.voterId;
  const password = await dbwrapper.getPasswordHash(voterId);
  const hashedPassword = getPasswordHash(req.body.password);
  if (hashedPassword != password) {
    res.status(401).send("Invalid password").end;
    return;
  }

  let questionIndices = req.body.questionIndices;
  if (!questionIndices || questionIndices.length === 0) {    
    res.json({ 'tokens': [] }).end;
    return;
  }
  
  if (typeof questionIndices[0] === "string" || questionIndices[0] instanceof String) {
    questionIndices = questionIndices.map(x => parseInt(x));
  }

  const tokens = await dbwrapper.getVotingTokens(voterId);
  const filteredTokens = tokens.filter(token => questionIndices.includes(token.tokenIndex));
  res.json({ 'tokens': filteredTokens }).end;
}));

app.post('/getToken', wrapAsync(async function (req, res) {
  const password = req.body.password;
  const voterId = req.body.voterId;
  const questionIndex = req.body.questionIndex;

  const dbPassword = await dbwrapper.getPasswordHash(voterId);
  const hashedPassword = getPasswordHash(password);
  if (hashedPassword != dbPassword) {
    res.status(401).send("Invalid password").end;
  }

  const voting = await dbwrapper.getToken(voterId, questionIndex);
  const token = voting.token;
  const blindingFactor = voting.blindingFactor;
  const signedToken = voting.signedToken;
  const key = await dbwrapper.getKey(voting.votingId, questionIndex);
  const unblindedToken = tokengenerator.unblindToken(signedToken, blindingFactor, password + '_' + questionIndex.toString() + '_' + voting.votingId, key.modulus);

  res.json({ 'unblindedToken': unblindedToken, 'token': token }).end;
}));

app.post('/saveRegistrationDetails', wrapAsync(async function (req, res) {
  const registrationId = req.body.id;
  const email = req.body.mail;
  const password = req.body.password;
  let tokenCount = req.body.tokenCount;
  
  if (tokenCount < 1) {
    tokenCount = 1; // TODO err?
  }
  if (tokenCount > MAX_TOKEN_COUNT) {
    tokenCount = MAX_TOKEN_COUNT;
  }

  let tokens = [];
  let blindedTokens = [];
  let blindingFactors = [];
  const keys = await dbwrapper.getKeys(req.body.votingId);
  for (let i = 0; i < tokenCount; i++) {
    const token = tokengenerator.generateToken();
    const blindedToken = tokengenerator.blindToken(token, password + '_' + i.toString() + '_' + req.body.votingId, keys[i].modulus, keys[i].exponent);
    blindingFactors.push(blindedToken.r);
    blindedTokens.push(blindedToken.blinded);
    tokens.push(token);
  }

  log.entry({ resource: logResource }, 'Saving Registration details for registration with id ' + registrationId);
  await dbwrapper.registerTokens(registrationId, email, tokens, blindedTokens, blindingFactors);
  await dbwrapper.savePasswordHash(registrationId, getPasswordHash(password));

  res.status(200).send('OK!').end;
}));

app.post('/sessionLogin', (req, res) => {
  // Get the ID token passed, TODO: check origin of req!
  const idToken = req.body.idToken.toString();

  // Set session expiration to 5 days.
  const expiresIn = 60 * 60 * 24 * 5 * 1000;
  // Create the session cookie. This will also verify the ID token in the process.
  // The session cookie will have the same claims as the ID token.
  // To only allow session cookie setting on recent sign-in, auth_time in ID token
  // can be checked to ensure user was recently signed in before creating a session cookie.
  admin.auth().createSessionCookie(idToken, { expiresIn }).then((sessionCookie) => {
    // Set cookie policy for session cookie.
    const options = { maxAge: expiresIn, httpOnly: true, secure: true };
    res.cookie('session', sessionCookie, options);
    res.end(JSON.stringify({ status: 'success', session: sessionCookie, maxAge: expiresIn }));
  }, error => {
    console.log(JSON.stringify(error));
    res.status(401).send('UNAUTHORIZED REQUEST!');
  });
});

// Start the server
if (!module.parent) {
  const PORT = process.env.PORT || 8082;
  app.listen(PORT, () => {
    console.log(`App listening on port ${PORT}`); // eslint-disable-line no-console
    console.log('Press Ctrl+C to quit.');         // eslint-disable-line no-console
  });
}

app.use(function (err, req, res, next) {
  // TODO: logging
  console.log('An error occurred: ' + err.message + ', stack: ' + err.stack); // eslint-disable-line no-console
  log.error('An error occurred: ' + err.message + ', stack: ' + err.stack);
  if (res.headersSent) {
    return next(err);
  }

  res.status(500);
  res.send('error: ' + err.message);
  res.end;
});
// [END app]

function allowCrossDomain(req, res, next) {
  if (req.headers.origin) {
    var origin = req.headers.origin.toLowerCase();

    if (origin.includes('localhost') || origin.includes('freiewahl')) {
      res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
      res.setHeader('Access-Control-Allow-Origin', origin);
    }
  }

  if (req.method === 'OPTIONS') {
    res.send(200);
  } else {
    next();
  }
}

function getPasswordHash(password) {
  return sha256(password + '_yummySalt420++');
}

function wrapAsync(fn) {
  return function (req, res, next) {
    // Make sure to `.catch()` any errors and pass them along to the `next()`
    // middleware in the chain, in this case the error handler.
    fn(req, res, next).catch(next);
  };
}

module.exports = app;
