const expect = require('chai').expect;
const assert = require('chai').assert;

const dbwrapper = require('./../dbwrapper');
const datastore = dbwrapper.datastore;

const timeout = ms => new Promise(res => setTimeout(res, ms));

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';
const TABLE_PUBLICKEYS = 'publicKeys';

const DATASTORE_WAIT_TIME = 100;

describe('The registerTokens function', function () {
  after(async() => {
    await deleteRegistration('rTestReg');
  });
  it('adds a list of tokens', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];

    // act
    await dbwrapper.registerTokens('rTestReg', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // assert
    const query = datastore.createQuery(TABLE_REGISTRATIONS)
      .filter('registrationId', '=', 'rTestReg');

    const results = await datastore.runQuery(query);
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
  });
});

describe('The getRegistration function', function() {
  after(async() => {
    await deleteRegistration('rTestGetReg0');
    await deleteRegistration('rTestGetReg1');
  });
  it('should return null/undefined if there is no matching registration', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetReg0', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const reg = await dbwrapper.getRegistration('foobar'); // different reg id

    // assert
    assert(!reg);
  });
  it('should return a registration if there is one matching', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetReg1', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

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
  });
});

describe('The setChallengeAndGetTokens function', function() {
  after(async() => {
    await deleteRegistration('rTestSetCha0');
    await deleteRegistration('rTestSetCha1');
  });
  it('sets a challenge and returns the blinded tokens', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestSetCha0', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const fetchedBlindedTokens = await dbwrapper.setChallengeAndGetTokens('rTestSetCha0', 'cha1234', Date.now().toString());

    // assert
    expect(fetchedBlindedTokens.length).to.equal(3);
    expect(fetchedBlindedTokens[0]).to.equal('blinded1');
    expect(fetchedBlindedTokens[1]).to.equal('blinded2');
    expect(fetchedBlindedTokens[2]).to.equal('blinded3');
  });
  it('returns null/undefined for a non-existent registration', async () => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestSetCha1', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const fetchedBlindedTokens = await dbwrapper.setChallengeAndGetTokens('badRegNumber', 'cha1234', Date.now().toString());

    assert(!fetchedBlindedTokens);
  });
});

describe('The deleteRegistration function', function() {
  after(async() => {
    await deleteRegistration('rTestDelReg0');
    await deleteRegistration('rTestDelReg1');
  });
  it('should delete a registration', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestDelReg0', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    await dbwrapper.deleteRegistration('rTestDelReg0');

    // assert
    await timeout(DATASTORE_WAIT_TIME);
    const reg = await dbwrapper.getRegistration('rTestDelReg0');
    assert(!reg);
  });
  it('should not fail when deleting a non-existing registration', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestDelReg1', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    await dbwrapper.deleteRegistration('badRegNumber');

    // assert
    const reg = await dbwrapper.getRegistration('rTestDelReg1');
    assert(reg);
  });
});

describe('The insertVotingTokens function', function() {
  after(async() => {
    await deleteVotingTokens('testVotingInsert1');
  });
  it('should insert a list of voting tokens', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const signed = [ 'signed1', 'signed2', 'signed3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];

    // act
    await dbwrapper.insertVotingTokens('testVotingInsert1', 'voter345', tokens, signed, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // assert
    const query = datastore.createQuery(TABLE_VOTINGTOKENS)
      .filter('votingId', '=', 'testVotingInsert1');
    const votingTokens = await datastore.runQuery(query);

    expect(votingTokens[0].length).to.equal(3);

    expect(votingTokens[0][0].votingId).to.equal('testVotingInsert1');
    expect(votingTokens[0][0].voterId).to.equal('voter345');
    expect(votingTokens[0][0].tokenIndex).to.equal(0);
    expect(votingTokens[0][0].token).to.equal('token1');
    expect(votingTokens[0][0].signedToken).to.equal('signed1');
    expect(votingTokens[0][0].blindingFactor).to.equal('blinding1');

    expect(votingTokens[0][1].votingId).to.equal('testVotingInsert1');
    expect(votingTokens[0][1].voterId).to.equal('voter345');
    expect(votingTokens[0][1].tokenIndex).to.equal(1);
    expect(votingTokens[0][1].token).to.equal('token2');
    expect(votingTokens[0][1].signedToken).to.equal('signed2');
    expect(votingTokens[0][1].blindingFactor).to.equal('blinding2');

    expect(votingTokens[0][2].votingId).to.equal('testVotingInsert1');
    expect(votingTokens[0][2].voterId).to.equal('voter345');
    expect(votingTokens[0][2].tokenIndex).to.equal(2);
    expect(votingTokens[0][2].token).to.equal('token3');
    expect(votingTokens[0][2].signedToken).to.equal('signed3');
    expect(votingTokens[0][2].blindingFactor).to.equal('blinding3');
  });
});

describe('The getToken method', function() {
  after(async() => {
    await deleteVotingTokens('testVotingGetToken1');
    await deleteVotingTokens('testVotingGetToken2');
  });
  it('gets a token previously inserted', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const signed = [ 'signed1', 'signed2', 'signed3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.insertVotingTokens('testVotingGetToken1', 'voter345', tokens, signed, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const token1 = await dbwrapper.getToken('voter345', 0);
    const token2 = await dbwrapper.getToken('voter345', 1);
    const token3 = await dbwrapper.getToken('voter345', 2);

    expect(token1.votingId).to.equal('testVotingGetToken1');
    expect(token1.voterId).to.equal('voter345');
    expect(token1.tokenIndex).to.equal(0);
    expect(token1.token).to.equal('token1');
    expect(token1.signedToken).to.equal('signed1');
    expect(token1.blindingFactor).to.equal('blinding1');

    expect(token2.votingId).to.equal('testVotingGetToken1');
    expect(token2.voterId).to.equal('voter345');
    expect(token2.tokenIndex).to.equal(1);
    expect(token2.token).to.equal('token2');
    expect(token2.signedToken).to.equal('signed2');
    expect(token2.blindingFactor).to.equal('blinding2');

    expect(token3.votingId).to.equal('testVotingGetToken1');
    expect(token3.voterId).to.equal('voter345');
    expect(token3.tokenIndex).to.equal(2);
    expect(token3.token).to.equal('token3');
    expect(token3.signedToken).to.equal('signed3');
    expect(token3.blindingFactor).to.equal('blinding3');
  }),
  it('does not fail when there is no such token', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const signed = [ 'signed1', 'signed2', 'signed3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.insertVotingTokens('testVotingGetToken2', 'voter456', tokens, signed, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const token1 = await dbwrapper.getToken('badVoterId', 0);
    const token2 = await dbwrapper.getToken('voter456', 5);

    // assert
    assert(!token1);
    assert(!token2);
  });
});

describe('The deleteTokens function', function() {
  it('should delete all tokens for a voting', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const signed = [ 'signed1', 'signed2', 'signed3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.insertVotingTokens('testVotingDeleteToken2', 'voter456', tokens, signed, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    await dbwrapper.deleteTokens('testVotingDeleteToken2');
    await timeout(DATASTORE_WAIT_TIME);

    // assert
    const query = datastore.createQuery(TABLE_VOTINGTOKENS)
      .filter('votingId', '=', 'testVotingDeleteToken2')
      .select('__key__');

    const results = await datastore.runQuery(query);
    expect(results[0].length).to.equal(0);
  });
});

describe('The insertKeys function', function() {
  after(async() => {
    await deleteKeys('vTestInsertKeys1');
  });
  it('should insert keys', async() => {
    // arrange
    const votingId = 'vTestInsertKeys1';
    const exponents = ['65537', '65538', '65539'];
    const moduli = ['1234abcd', '4321dcba', '98765432'];

    // act
    await dbwrapper.insertKeys(votingId, exponents, moduli);
    await timeout(DATASTORE_WAIT_TIME);

    // assert
    const query = datastore.createQuery(TABLE_PUBLICKEYS)
      .order('keyIndex');
    const results = await datastore.runQuery(query);
    expect(results[0].length).to.equal(3);

    expect(results[0][0].votingId).to.equal('vTestInsertKeys1');
    expect(results[0][0].keyIndex).to.equal(0);
    expect(results[0][0].exponent).to.equal('65537');
    expect(results[0][0].modulus).to.equal('1234abcd');

    expect(results[0][1].votingId).to.equal('vTestInsertKeys1');
    expect(results[0][1].keyIndex).to.equal(1);
    expect(results[0][1].exponent).to.equal('65538');
    expect(results[0][1].modulus).to.equal('4321dcba');

    expect(results[0][2].votingId).to.equal('vTestInsertKeys1');
    expect(results[0][2].keyIndex).to.equal(2);
    expect(results[0][2].exponent).to.equal('65539');
    expect(results[0][2].modulus).to.equal('98765432');
  });
});

describe('The getKey function', function() {
  after(async() => {
    await deleteKeys('vTestGetKey0');
    await deleteKeys('vTestGetKey1');
    await deleteKeys('vTestGetKey2');
    await deleteKeys('vTestGetKey3');
  });
  it('gets a keys with a given voting id and index', async() => {
    // arrange
    const votingId = 'vTestGetKey0';
    const exponents = ['65537', '65538', '65539'];
    const moduli = ['1234abcd', '4321dcba', '98765432'];
    const votingId2 = 'vTestGetKey1';
    const exponents2 = ['165537', '165538', '165539'];
    const moduli2 = ['1234abcda', '4321dcbab', '98765432c'];
    await dbwrapper.insertKeys(votingId, exponents, moduli);
    await timeout(DATASTORE_WAIT_TIME);
    await dbwrapper.insertKeys(votingId2, exponents2, moduli2);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const key0 = await dbwrapper.getKey(votingId, 0);
    const key1 = await dbwrapper.getKey(votingId2, 1);
    const key2 = await dbwrapper.getKey(votingId, 2);

    // assert

    expect(key0.votingId).to.equal('vTestGetKey0');
    expect(key0.keyIndex).to.equal(0);
    expect(key0.exponent).to.equal('65537');
    expect(key0.modulus).to.equal('1234abcd');

    expect(key1.votingId).to.equal('vTestGetKey1');
    expect(key1.keyIndex).to.equal(1);
    expect(key1.exponent).to.equal('165538');
    expect(key1.modulus).to.equal('4321dcbab');

    expect(key2.votingId).to.equal('vTestGetKey0');
    expect(key2.keyIndex).to.equal(2);
    expect(key2.exponent).to.equal('65539');
    expect(key2.modulus).to.equal('98765432');
  });
  it('gets nothing when there is no matching key', async() => {
    // arrange
    const votingId = 'vTestGetKey2';
    const exponents = ['65537', '65538'];
    const moduli = ['1234abcd', '4321dcba'];
    const votingId2 = 'vTestGetKey3';
    const exponents2 = ['165537', '165538', '165539'];
    const moduli2 = ['1234abcda', '4321dcbab', '98765432c'];
    await dbwrapper.insertKeys(votingId, exponents, moduli);
    await timeout(DATASTORE_WAIT_TIME);
    await dbwrapper.insertKeys(votingId2, exponents2, moduli2);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const key0 = await dbwrapper.getKey('badVotingId', 0);
    const key1 = await dbwrapper.getKey(votingId, 2);
    const key2 = await dbwrapper.getKey(votingId2, 3);

    // assert
    assert(!key0);
    assert(!key2);
    assert(!key1);
  });
});

describe('The getKeys function', function() {
  after(async() => {
    await deleteKeys('vTestGetKeys0');
    await deleteKeys('vTestGetKeys1');
    await deleteKeys('vTestGetKeys2');
    await deleteKeys('vTestGetKeys3');
  });
  it('gets all keys with a given voting id', async() => {
    // arrange
    const votingId = 'vTestGetKeys0';
    const exponents = ['65537', '65538', '65539'];
    const moduli = ['1234abcd', '4321dcba', '98765432'];
    const votingId2 = 'vTestGetKeys1';
    const exponents2 = ['165537', '165538', '165539'];
    const moduli2 = ['1234abcda', '4321dcbab', '98765432c'];
    await dbwrapper.insertKeys(votingId, exponents, moduli);
    await timeout(DATASTORE_WAIT_TIME);
    await dbwrapper.insertKeys(votingId2, exponents2, moduli2);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const keys = await dbwrapper.getKeys(votingId);

    // assert
    expect(keys.length).to.equal(3);

    expect(keys[0].votingId).to.equal('vTestGetKeys0');
    expect(keys[0].keyIndex).to.equal(0);
    expect(keys[0].exponent).to.equal('65537');
    expect(keys[0].modulus).to.equal('1234abcd');

    expect(keys[1].votingId).to.equal('vTestGetKeys0');
    expect(keys[1].keyIndex).to.equal(1);
    expect(keys[1].exponent).to.equal('65538');
    expect(keys[1].modulus).to.equal('4321dcba');

    expect(keys[2].votingId).to.equal('vTestGetKeys0');
    expect(keys[2].keyIndex).to.equal(2);
    expect(keys[2].exponent).to.equal('65539');
    expect(keys[2].modulus).to.equal('98765432');
  }),
  it('gets nothing when there are no keys for a voting id', async() => {
    // arrange
    const votingId = 'vTestGetKeys2';
    const exponents = ['65537', '65538', '65539'];
    const moduli = ['1234abcd', '4321dcba', '98765432'];
    const votingId2 = 'vTestGetKeys3';
    const exponents2 = ['165537', '165538', '165539'];
    const moduli2 = ['1234abcda', '4321dcbab', '98765432c'];
    await dbwrapper.insertKeys(votingId, exponents, moduli);
    await timeout(DATASTORE_WAIT_TIME);
    await dbwrapper.insertKeys(votingId2, exponents2, moduli2);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    const keys = await dbwrapper.getKeys('badVotingId');

    // assert
    assert(!keys || keys.length == 0);
  });
});

describe('The getChallenge function', function() {
  after(async() => {
    await deleteRegistration('rTestGetCha0');
    await deleteRegistration('rTestGetCha1');
    await deleteRegistration('rTestGetCha2');
    await deleteRegistration('rTestGetCha3');
  });
  it('should get the challenge previously set', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha0', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha0', 'cha1234', Date.now().toString());
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha0');

    // assert
    expect(challenge).to.equal('cha1234');
  });
  it('returns null/undefined if the date difference is too large', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha1', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha1', 'cha1234', (Date.now() - 200000).toString());
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha1');

    // assert
    assert(!challenge);
  });
  it('returns null/undefined if there is no such registration', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha2', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await dbwrapper.setChallengeAndGetTokens('rTestGetCha2', 'cha1234', (Date.now() - 200000).toString());
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var challenge = await dbwrapper.getChallenge('badRegId');

    // assert
    assert(!challenge);
  });
  it('returns null/undefined if no challenge was set', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const blinded = [ 'blinded1', 'blinded2', 'blinded3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.registerTokens('rTestGetCha3', 'michivo@gmail.com', tokens, blinded, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var challenge = await dbwrapper.getChallenge('rTestGetCha3');

    // assert
    assert(!challenge);
  });
});


// BEGIN helper functions
// async function printTable(table) {
//   const query = datastore.createQuery(table);

//   return datastore.runQuery(query)
//   .then((results) => {
//     results[0].forEach(x => console.log(JSON.stringify(x)));
//   });
// }

function deleteKeys(votingId) {
  const query = datastore.createQuery(TABLE_PUBLICKEYS)
    .filter('votingId', '=', votingId)
    .select('__key__');

  return datastore.runQuery(query)
    .then((results) => {
      datastore.delete(results[0].map(x => x[datastore.KEY]));
    });
}

async function deleteRegistration(registrationId) {
  const query = datastore.createQuery(TABLE_REGISTRATIONS)
    .filter('registrationId', '=', registrationId)
    .select('__key__');

  return datastore.runQuery(query)
    .then((results) => {
      datastore.delete(results[0].map(x => x[datastore.KEY]));
    });
}

async function deleteVotingTokens(votingId) {
  const query = datastore.createQuery(TABLE_VOTINGTOKENS)
    .filter('votingId', '=', votingId)
    .select('__key__');

  return datastore.runQuery(query).then((results) => { // TODO: error handling?
    datastore.delete(results[0].map(x => x[datastore.KEY]));
  });
}

// END helper functions
