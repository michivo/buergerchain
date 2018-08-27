const Datastore = require('@google-cloud/datastore');
const config = require('./config.json');

const datastore = Datastore();

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';

async function addRegisteredTokens(registrationId, email, tokens, blindingFactors) {
    const newRegistration = {
        timestamp: new Date(),
        registrationId: registrationId,
        email: email,
        tokens: tokens,
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

    return datastore.runQuery(query)
        .then((results) => { // TODO: error handling
            datastore.delete(results[0].map(x => x[datastore.KEY]));
        })
}

function clearTokensForVoting(votingId) {
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
        .filter('votingId', '=', votingId);

    return datastore.runQuery(query)
        .then((results) => { // TODO: error handling
            datastore.delete(results[0].map(x => x[datastore.KEY]));
        })
}

function getTokens(votingId, voterId) {
    const query = datastore.createQuery(TABLE_VOTINGTOKENS)
        .filter('voterId', '=', voterId)
        .limit(1);

    return datastore.runQuery(query)
        .then((results) => {
            // TODO: error handling
            var resultArray = results[0];
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
      var queryResult = results[0];
      if (queryResult.length != 1) {
          // TODO error
      }
      var registration = queryResult[0];
      registration.challenge = challenge;
      registration.date = date;
      await datastore.save(registration);
      return registration.tokens;
  })
}

function getChallenge(registrationId, challenge, date) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
      .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query).then(async (results) => {
      var queryResult = results[0];
      if (queryResult.length != 1) {
          return null; // TODO logging
      }
      var registration = queryResult[0];
      var now = Date.now();
      if(now - date > config.MAX_TIME_DELTA) {
        return null; // TODO logging
      }

      return registration.challenge;
  })
}

function getRegistration(registrationId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
      .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query).then(async (results) => {
      var queryResult = results[0];
      if (queryResult.length != 1) {
          return null; // TODO logging
      }
      return queryResult[0];
    });
}

async function insertVotingTokens(votingId, voterId, signedTokens, blindingFactors) {
  const newRegistration = {
      timestamp: new Date(),
      votingId: votingId,
      voterId: voterId,
      signedTokens: signedTokens,
      blindingFactors: blindingFactors
  };
  await datastore.save({
      key: datastore.key(TABLE_VOTINGTOKENS),
      data: newRegistration
  });
}

function mapToken(origToken, voterId, votingId) {
    return {
        key: datastore.key(TABLE_VOTINGTOKENS),
        data: {
            index: origToken.index,
            tokenId: origToken.tokenId,
            blindingFactor: origToken.blindingFactor
        }
    };
}


module.exports = {
    registerTokens: addRegisteredTokens,
    getTokens: getTokens,
    setChallengeAndGetTokens: setChallengeAndGetTokens,
    getChallenge: getChallenge,
    getRegistration: getRegistration,
    insertVotingTokens: insertVotingTokens,
    deleteRegistration: deleteRegistration
}
