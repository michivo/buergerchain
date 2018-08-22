const Datastore = require('@google-cloud/datastore');
const config = require('./config.json');

const datastore = Datastore();

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';

function hello() {
    return "hello dbwrapper!";
}

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

function clearTokensForVoting(votingId) {
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
        .filter('votingId', '=', votingId);

    return datastore.runQuery(query)
        .then((results) => { // TODO: error handling
            datastore.delete(results[0].map(x => x[datastore.KEY]));
        })
}

function getToken(votingId, voterId, index) {
    const query = datastore.createQuery(TABLE_VOTINGTOKENS)
        .filter('voterId', '=', voterId)
        .filter('votingId', '=', votingId)
        .filter('index', '=', index);

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

function setChallenge(registrationId, challenge, date) {
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
      console.log(registration);
      await datastore.save(registration);
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

function unlockVoter(registrationId, votingId, voterId) {
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
        .filter('registrationId', '=', registrationId);

    return datastore.runQuery(query).then(async (results) => {
        var queryResult = results[0];
        if (queryResult.length != 1) {
            // TODO error
        }

        var tokens = queryResult[0].tokens.map(x => mapToken(x));

        await datastore.save(tokens);
        // TODO error handling, api response stuff
    })
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
    hello: hello,
    registerTokens: addRegisteredTokens,
    clearTokens: clearTokensForVoting,
    unlockVoter: unlockVoter,
    getToken: getToken,
    datastore: datastore,
    setChallenge: setChallenge,
    getChallenge: getChallenge
}
