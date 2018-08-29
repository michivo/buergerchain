const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const Datastore = require('@google-cloud/datastore');
const dbwrapper = require('./../dbwrapper');

const timeout = ms => new Promise(res => setTimeout(res, ms));

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';

describe('The registerTokens function', function () {
  after(async() => {
    await deleteRegistration(dbwrapper.datastore, 'rTestReg');
  })
  it('adds a list of tokens', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];

    // act
    await dbwrapper.registerTokens('rTestReg', 'michivo@gmail.com', tokens, blinded, blindingFactors);

    // assert
    const query = dbwrapper.datastore.createQuery(TABLE_REGISTRATIONS)
    .filter('registrationId', '=', 'rTestReg');

    const results = await dbwrapper.datastore.runQuery(query);
    const queryResults = results[0];
    expect(queryResults.length).to.equal(1);
    const result = queryResults[0];
    expect(result.registrationId).to.equal('rTestReg');
    expect(result.email).to.equal('michivo@gmail.com');
    expect(result.tokens.length).to.equal(3);
    expect(result.blindedTokens.length).to.equal(3);
    expect(result.blindingFactors.length).to.equal(3);

    expect(result.tokens[0]).to.equal('token1');
    expect(result.tokens[1]).to.equal('token2');
    expect(result.tokens[2]).to.equal('token3');

    expect(result.blindedTokens[0]).to.equal('blinded1');
    expect(result.blindedTokens[1]).to.equal('blinded2');
    expect(result.blindedTokens[2]).to.equal('blinded3');

    expect(result.blindingFactors[0]).to.equal('blinding1');
    expect(result.blindingFactors[1]).to.equal('blinding2');
    expect(result.blindingFactors[2]).to.equal('blinding3');
  })
})

describe('The getRegistration function', function() {
  after(async() => {
    await deleteRegistration(dbwrapper.datastore, 'rTestGetReg0');
    await deleteRegistration(dbwrapper.datastore, 'rTestGetReg1');
  })
  it('should return null/undefined if there is no matching registration', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetReg0', 'michivo@gmail.com', tokens, blinded, blindingFactors);

    // act
    const reg = await dbwrapper.getRegistration('foobar'); // different reg id

    // assert
    assert(!reg);
  })
  it('should return a registration if there is one matching', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetReg1', 'michivo@gmail.com', tokens, blinded, blindingFactors);

    // act
    const result = await dbwrapper.getRegistration('rTestGetReg1'); // same reg id

    // assert
    expect(result.registrationId).to.equal('rTestGetReg1');
    expect(result.email).to.equal('michivo@gmail.com');
    expect(result.tokens.length).to.equal(3);
    expect(result.blindedTokens.length).to.equal(3);
    expect(result.blindingFactors.length).to.equal(3);

    expect(result.tokens[0]).to.equal('token1');
    expect(result.tokens[1]).to.equal('token2');
    expect(result.tokens[2]).to.equal('token3');

    expect(result.blindedTokens[0]).to.equal('blinded1');
    expect(result.blindedTokens[1]).to.equal('blinded2');
    expect(result.blindedTokens[2]).to.equal('blinded3');

    expect(result.blindingFactors[0]).to.equal('blinding1');
    expect(result.blindingFactors[1]).to.equal('blinding2');
    expect(result.blindingFactors[2]).to.equal('blinding3');
  })
})

describe('The setChallengeAndGetTokens function', function() {
  after(async() => {
    await deleteRegistration(dbwrapper.datastore, 'rTestSetCha0');
    await deleteRegistration(dbwrapper.datastore, 'rTestSetCha1');
  })
  it('sets a challenge and returns the blinded tokens', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestSetCha0', 'michivo@gmail.com', tokens, blinded, blindingFactors);

    // act
    const fetchedBlindedTokens = await dbwrapper.setChallengeAndGetTokens('rTestSetCha0', 'cha1234', Date.now().toString());

    // assert
    expect(tokens.length).to.equal(3);
    expect(fetchedBlindedTokens[0]).to.equal('blinded1');
    expect(fetchedBlindedTokens[1]).to.equal('blinded2');
    expect(fetchedBlindedTokens[2]).to.equal('blinded3');
  })
  it('returns null/undefined for a non-existent registration', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestSetCha1', 'michivo@gmail.com', tokens, blinded, blindingFactors);

    // act
    const fetchedBlindedTokens = await dbwrapper.setChallengeAndGetTokens('badRegNumber', 'cha1234', Date.now().toString());

    assert(!fetchedBlindedTokens);
  })
})

describe('The getChallenge function', function() {
  after(async() => {
    await deleteRegistration(dbwrapper.datastore, 'rTestGetCha0');
    await deleteRegistration(dbwrapper.datastore, 'rTestGetCha1');
    await deleteRegistration(dbwrapper.datastore, 'rTestGetCha2');
    await deleteRegistration(dbwrapper.datastore, 'rTestGetCha3');
  })
  it('should get the challenge previously set', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha0', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha0', 'cha1234', Date.now().toString());
    await timeout(100);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha0');

    // assert
    expect(challenge).to.equal('cha1234');
  })
  it('returns null/undefined if the date difference is too large', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha1', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha1', 'cha1234', (Date.now() - 200000).toString());
    await timeout(100);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha1');

    // assert
    assert(!challenge);
  })
  it('returns null/undefined if there is no such registration', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha2', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha2', 'cha1234', (Date.now() - 200000).toString());
    await timeout(100);

    // act
    var challenge = await dbwrapper.getChallenge('badRegId');

    // assert
    assert(!challenge);
  })
  it('returns null/undefined if no challenge was set', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha3', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(100);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha3');

    // assert
    assert(!challenge);
  })
})

async function printTable(datastore, table) {
  const query = datastore.createQuery(table);

  return datastore.runQuery(query)
  .then((results) => {
    results[0].forEach(x => console.log(JSON.stringify(x)));
  })
}

async function deleteRegistration(datastore, registrationId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
    .filter('registrationId', '=', registrationId);

  return datastore.runQuery(query)
  .then((results) => {
    datastore.delete(results[0].map(x => x[datastore.KEY]));
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
