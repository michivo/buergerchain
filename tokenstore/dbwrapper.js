const Datastore = require('@google-cloud/datastore');
const config = require('./config.json');

const datastore = Datastore({ projectId: config.GCLOUD_PROJECT });

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';


async function addRegisteredTokens(registrationId, email, tokens, blindedTokens, blindingFactors) {
  const newRegistration = {
    timestamp: new Date(),
    registrationId: registrationId,
    email: email,
    tokens: tokens,
    blindedTokens: blindedTokens,
    blindingFactors: blindingFactors
  };

  await datastore.save({
    key: datastore.key(TABLE_REGISTRATIONS),
    data: newRegistration
  });

}

function deleteRegistration(regId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
  .filter('registrationId', '=', regId)
  .select('__key__')
  .limit(1);

  return datastore.runQuery(query).then((results) => { // TODO: error handling?
    datastore.delete(results[0].map(x => x[datastore.KEY]));
  })
}

function getToken(votingId, voterId, index) {
  const query = datastore.createQuery(TABLE_VOTINGTOKENS)
  .filter('voterId', '=', voterId)
  .filter('tokenIndex', '=', index)
  .limit(1);

  return datastore.runQuery(query)
  .then((results) => {
    // TODO: error handling
    const resultArray = results[0];
    if(resultArray.length != 1) {
      return null;
    }

    return resultArray[0];
  });
}

function setChallengeAndGetTokens(registrationId, challenge, date) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
  .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query).then(async (results) => {
    const queryResult = results[0];
    if (queryResult.length != 1) {
      // TODO error
      return null;
    }
    let registration = queryResult[0];
    registration.challenge = challenge;
    registration.date = date;
    await datastore.save(registration);
    return registration.blindedTokens;
  })
}

function getChallenge(registrationId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
  .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query).then(async (results) => {
    const queryResult = results[0];
    if (queryResult.length != 1) {
      return null; // TODO logging
    }
    const registration = queryResult[0];
    const now = Date.now();
    if(now - registration.date > config.MAX_TIME_DELTA) {
      return null; // TODO logging
    }

    return registration.challenge;
  })
}

function getRegistration(registrationId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
    .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query).then(async (results) => {
    let queryResult = results[0];
    if (queryResult.length != 1) {
      return null; // TODO logging
    }

    return queryResult[0];
  });
}

async function insertVotingTokens(votingId, voterId, tokens, signedTokens, blindingFactors) {
  let tokenEntities = [];
  for(let i = 0; i < tokens.length; i++) {
    const newEntity = {
      key: datastore.key(TABLE_VOTINGTOKENS),
      data: {
        timestamp: Date.now().toString(),
        votingId: votingId,
        voterId: voterId,
        tokenIndex: i,
        token: tokens[i],
        signedToken: signedTokens[i],
        blindingFactor: blindingFactors[i]
      }
    };
    tokenEntities[i] = newEntity;
  }
  await datastore.insert(tokenEntities);
}

module.exports = {
  registerTokens: addRegisteredTokens,
  getToken: getToken,
  setChallengeAndGetTokens: setChallengeAndGetTokens,
  getChallenge: getChallenge,
  getRegistration: getRegistration,
  insertVotingTokens: insertVotingTokens,
  deleteRegistration: deleteRegistration,
  datastore: datastore
}
