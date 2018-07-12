const Datastore = require('@google-cloud/datastore');

const datastore = Datastore();

function hello() {
    return "hello dbwrapper!";
}

function addRegisteredTokens(registrationId, votingId, email, tokens) {
    const newRegistration = {
        timestamp: new Date(),
        registrationId: registrationId,
        votingId: votingId,
        email: email,
        tokens: tokens
    };
    return datastore.save({
        key: datastore.key('registration'),
        data: newRegistration
    });
}

function clearTokensForVoting(votingId) {
    const query = datastore.createQuery('registration')
        .filter('votingId', '=', votingId);

    return datastore.runQuery(query)
        .then((results) => {
            datastore.delete(results[0].map(x => x[datastore.KEY]));
        })
}

function getRegisteredTokens(votingId) {
    const query = datastore.createQuery('registration')
        .filter('votingId', '=', votingId);

    return datastore.runQuery(query)
        .then((results) => {
            return results[0];
        })
}

function getToken(votingId, voterId) {

}

function unlockToken(registrationId, votingId, voterId) {
    const query = datastore.createQuery('registration')
        .filter('votingId', '=', votingId)
        .filter('registrationId', '=', registrationId);

    datastore.runQuery(query).then((results) => {
        var queryResult = results[0];
        if (queryResult.length != 1) {
            // TODO error
        }

        var tokens = queryResult[0].map(mapToken);

        datastore.save(tokens);
        // TODO error handling, api response stuff
    })
}

function mapToken(origToken) {
    return {
        key: datastore.key('votingTokens'),
        data: {
            index: x.index,
            tokenId: x.tokenId,
            blindingFactor: x.blindingFactor,
            voterId: x.voterId,
            votingId: x.votingId
        }
    };
}


module.exports = {
    hello: hello,
    registerTokens: addRegisteredTokens,
    getRegisteredTokens: getRegisteredTokens,
    clearTokens: clearTokensForVoting,
    unlockToken: unlockToken
}