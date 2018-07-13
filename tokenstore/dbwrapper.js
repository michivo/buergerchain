const Datastore = require('@google-cloud/datastore');

const datastore = Datastore();

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';

function hello() {
    return "hello dbwrapper!";
}

async function addRegisteredTokens(registrationId, votingId, email, tokens) {
    const newRegistration = {
        timestamp: new Date(),
        registrationId: registrationId,
        votingId: votingId,
        email: email,
        tokens: tokens
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

function getRegisteredTokens(votingId) {
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
        .filter('votingId', '=', votingId);

    return datastore.runQuery(query)
        .then((results) => { // TODO: error handling
            return results[0];
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

function unlockVoter(registrationId, votingId, voterId) {
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
        .filter('votingId', '=', votingId)
        .filter('registrationId', '=', registrationId);

    return datastore.runQuery(query).then(async (results) => {
        var queryResult = results[0];
        if (queryResult.length != 1) {
            // TODO error
        }

        var tokens = queryResult[0].tokens.map(x => mapToken(x, voterId, votingId));

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
            blindingFactor: origToken.blindingFactor,
            voterId: voterId,
            votingId: votingId
        }
    };
}


module.exports = {
    hello: hello,
    registerTokens: addRegisteredTokens,
    getRegisteredTokens: getRegisteredTokens,
    clearTokens: clearTokensForVoting,
    unlockVoter: unlockVoter,
    getToken: getToken,
    datastore: datastore
}