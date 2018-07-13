const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const Datastore = require('@google-cloud/datastore');
const dbwrapper = require('./../dbwrapper')

describe('The hello function', function () {
    it('says hello', function () {
        var hello = dbwrapper.hello();

        expect(hello).to.equal('hello dbwrapper!');
    })
})

describe('The addToken function', function () {
    it('adds a list of tokens', async () => {
        await clearTable(dbwrapper.datastore, 'registration');
        var tokens = [
            { index: 1, tokenId: 't1234', blindingFactor: 'b123456' },
            { index: 2, tokenId: 't4321', blindingFactor: 'b654321' }
        ];

        await dbwrapper.registerTokens('r12345987', 'v12345', 'michivo@gmail.com', tokens);

        var result = await dbwrapper.getRegisteredTokens('v12345')
        expect(result.length).to.equal(1);
        expect(result[0].registrationId).to.equal('r12345987');
        expect(result[0].votingId).to.equal('v12345');
        expect(result[0].tokens.length).to.equal(2);

        var token1 = result[0].tokens.find(e => e.index === 1);
        var token2 = result[0].tokens.find(e => e.index === 2);

        expect(token1.tokenId).to.equal('t1234');
        expect(token1.blindingFactor).to.equal('b123456');
        expect(token2.tokenId).to.equal('t4321');
        expect(token2.blindingFactor).to.equal('b654321');
    })
})

describe('The unlockToken function', function() {
    it('should unlock previously registered tokens', async () => {
        await clearTable(dbwrapper.datastore, 'registration');
        await clearTable(dbwrapper.datastore, 'votingToken');
        var tokens = [
            { index: 1, tokenId: 't1234', blindingFactor: 'b123456' },
            { index: 2, tokenId: 't4321', blindingFactor: 'b654321' }
        ];

        await dbwrapper.registerTokens('r12345987', 'v12345', 'michivo@gmail.com', tokens);

        await dbwrapper.unlockVoter('r12345987', 'v12345', 'V54321');
        await printTable(dbwrapper.datastore, 'votingToken');
        var token = await dbwrapper.getToken('v12345', 'V54321', 1);
        expect(token.index).to.equal(1);
        expect(token.voterId).to.equal('V54321');
        expect(token.votingId).to.equal('v12345');
        expect(token.blindingFactor).to.equal('b123456');
        expect(token.tokenId).to.equal('t1234');

        var token = await dbwrapper.getToken('v12345', 'V54321', 2);
        expect(token.index).to.equal(2);
        expect(token.voterId).to.equal('V54321');
        expect(token.votingId).to.equal('v12345');
        expect(token.blindingFactor).to.equal('b654321');
        expect(token.tokenId).to.equal('t4321');        
    })
})

describe('The getToken function', function () {
    it('gets tokens with the given voting and voter ids', async () => {
        const datastore = dbwrapper.datastore;
        await clearTable(datastore, 'votingToken');
        await addTestTokens(datastore);

        var token = await dbwrapper.getToken('V4321', 'v1234', 1);
        expect(token.index).to.equal(1);
        expect(token.voterId).to.equal('v1234');
        expect(token.votingId).to.equal('V4321');
        expect(token.blindingFactor).to.equal('b5555');
        expect(token.tokenId).to.equal('t1234');


        token = await dbwrapper.getToken('V4321', 'v1234', 2);
        expect(token.index).to.equal(2);
        expect(token.voterId).to.equal('v1234');
        expect(token.votingId).to.equal('V4321');
        expect(token.blindingFactor).to.equal('b5678');
        expect(token.tokenId).to.equal('t9876');        
    })
    it('returns null if there is no such entry', async() => {
        const datastore = dbwrapper.datastore;
        await clearTable(datastore, 'votingToken');
        await addTestTokens(datastore);

        token = await dbwrapper.getToken('V4321', 'v1234', 3);
        expect(token).to.equal(null);
    });
});

async function printTable(datastore, table) {
    const query = datastore.createQuery(table);

    return datastore.runQuery(query)
        .then((results) => {
            results[0].forEach(x => console.log(JSON.stringify(x)));
        })
}


async function clearTable(datastore, table) {
    const query = datastore.createQuery(table);

    return datastore.runQuery(query)
        .then((results) => {
            datastore.delete(results[0].map(x => x[datastore.KEY]));
        })
}

async function addTestTokens(datastore) {
    await datastore.save({
        key: datastore.key('votingToken'),
        data: {
            index: 1,
            tokenId: 't1234',
            blindingFactor: 'b5555',
            voterId: 'v1234',
            votingId: 'V4321'
        }
    });
    await datastore.save({
        key: datastore.key('votingToken'),
        data: {
            index: 2,
            tokenId: 't9876',
            blindingFactor: 'b5678',
            voterId: 'v1234',
            votingId: 'V4321'
        }
    });
    await datastore.save({
        key: datastore.key('votingToken'),
        data: {
            index: 1,
            tokenId: 't4444',
            blindingFactor: 'b5432',
            voterId: 'v1243', // different voterId!
            votingId: 'V4321'
        }
    });         
}