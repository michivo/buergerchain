const expect = require('chai').expect;
const assert = require('chai').assert;

const dbwrapper = require('./../dbwrapper');
const datastore = dbwrapper.datastore;

const timeout = ms => new Promise(res => setTimeout(res, ms));

const TABLE_REGISTRATIONS = 'registration';
const TABLE_VOTINGTOKENS = 'votingToken';
const TABLE_PUBLICKEYS = 'publicKeys';
const TABLE_PASSWORDS = 'voterPasswords';

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

describe('Test', function() {
  it('should work', function() {
    const tokens = [{"tokenIndex":17,"token":"040863f0-d177-48e3-aa3c-12a91f579b10"},{"token":"052f18aa-7657-4e7f-b112-f91d969386ac","tokenIndex":13},{"token":"0656b0ef-be18-43fc-87b2-516f4a2ed9da","tokenIndex":67},{"tokenIndex":27,"token":"089e62ca-e2ad-42df-adf0-113b70d2341b"},{"tokenIndex":55,"token":"0aa64d7a-30b2-4f8b-814c-caf29b5ecd7d"},{"tokenIndex":83,"token":"0fda70be-0852-4821-9814-21cbd93d420c"},{"tokenIndex":80,"token":"11d41cbd-3ab1-43c0-94f2-304e5c0019c1"},{"tokenIndex":94,"token":"1292e78c-c3ef-4219-a0f3-f2e34e9154ca"},{"tokenIndex":32,"token":"159cfb53-8eb5-49e9-8897-93d24f92fde3"},{"token":"1f5392ca-f9ab-4696-b2a1-b4b9c95612ee","tokenIndex":85},{"token":"20091555-a6c2-456e-b962-46cf3334e884","tokenIndex":38},{"tokenIndex":4,"token":"22ee6e1a-1a34-4c7d-a840-f9f1854aeef2"},{"token":"276e5af8-212c-4b9d-a64e-43f147bb4f4b","tokenIndex":54},{"token":"284bcc1d-4c9b-41e7-8c6c-496efa80d50f","tokenIndex":73},{"token":"2ad70046-c1d0-440e-b51d-766f29644635","tokenIndex":40},{"token":"3131c913-180a-4435-8813-7c5d36282181","tokenIndex":15},{"tokenIndex":95,"token":"317c20c2-b280-41d8-8ab1-2620ed505a09"},{"tokenIndex":35,"token":"362fad82-7a4f-4e29-aaf0-d795d8863f8c"},{"token":"36571bd9-4a73-4d2c-904d-c0a246c4c6d2","tokenIndex":24},{"tokenIndex":52,"token":"3675cb62-a883-441b-a4e0-0bac015b0d81"},{"tokenIndex":99,"token":"3b232bff-38b5-4e37-bc0a-273097e07cc8"},{"tokenIndex":42,"token":"3de98df1-7a19-4b5a-9efe-fc64c6eae489"},{"tokenIndex":2,"token":"435e88b2-88e5-4f7d-912d-48b076c5ddd2"},{"token":"4378cbce-11a0-40bf-9639-4c3c253b02dd","tokenIndex":96},{"tokenIndex":65,"token":"461ea2f1-7a45-477e-9a7e-a9ef0ebead4e"},{"token":"4dbb620b-e1f2-45f7-80af-7a39c773633b","tokenIndex":49},{"token":"4fd57383-abeb-4524-80e8-092b1e4d8184","tokenIndex":46},{"tokenIndex":84,"token":"513713b1-deb7-4189-b39e-1312721cae26"},{"token":"516b097b-9e5c-4be7-ae4a-51ce6073fa49","tokenIndex":11},{"tokenIndex":3,"token":"527b9ad3-cdf9-4350-a04d-cc8d4a66734b"},{"token":"56974d19-b00f-4de2-8beb-455168b664ec","tokenIndex":60},{"token":"5731a666-d898-4155-ace4-c46fde424482","tokenIndex":47},{"tokenIndex":76,"token":"5afe8830-22e5-40a1-a146-0123fbe56393"},{"token":"5bdfdb5a-0c29-4b91-ac3c-4088a0864738","tokenIndex":59},{"token":"5e25f17c-83c5-42de-bf35-a8b65825df9d","tokenIndex":74},{"tokenIndex":36,"token":"5e4324d3-a157-4dad-9ae4-e9fad361528d"},{"tokenIndex":63,"token":"5e5ff8ae-9f04-4140-a5f4-ac9078ba4b0c"},{"tokenIndex":30,"token":"5f054675-0823-40b6-93fd-8aa1f28a841b"},{"tokenIndex":5,"token":"5fe0787a-41cc-47b8-9a5a-1f68639de415"},{"tokenIndex":86,"token":"5feb4797-75fa-4cb8-83a4-f9563c5c4317"},{"tokenIndex":29,"token":"6003e418-946c-4416-a431-6cedd25e8dce"},{"tokenIndex":23,"token":"62917b10-bbe2-4e4a-a7f6-36288bc3974d"},{"token":"6e8ae6ba-1673-488c-86c1-9cb194ed3272","tokenIndex":43},{"tokenIndex":64,"token":"72e824c6-dd45-439e-8f1c-71cbb8dbd449"},{"tokenIndex":20,"token":"73ba0a44-f661-4826-80a3-daac24682ae2"},{"tokenIndex":50,"token":"76171047-03c3-481e-8988-a14aab2347b1"},{"tokenIndex":44,"token":"766cea79-2254-4468-94e8-e0985895eca4"},{"token":"7738032d-e5a6-4e5d-8a30-74a8c0be7a8c","tokenIndex":93},{"tokenIndex":22,"token":"77af5d3d-9e27-42d9-b109-f92479341524"},{"token":"792458b0-97aa-4c26-96b9-755c3bf6627a","tokenIndex":87},{"token":"7f132269-0478-41a5-8219-824f1500dd46","tokenIndex":91},{"token":"80308b71-5fa3-46d7-a626-53e8bddb5fe3","tokenIndex":92},{"tokenIndex":28,"token":"8041b6df-5ee4-49eb-b101-681b15a82920"},{"token":"81392da2-a43b-49f9-8687-37fef103fdf8","tokenIndex":48},{"token":"82ee51f1-7e49-4813-be15-7ec496dc1f7d","tokenIndex":16},{"tokenIndex":26,"token":"87bf41f0-c524-4918-88b4-b98d8de371de"},{"token":"906eddee-f527-470c-8edc-21f5bdce4c02","tokenIndex":72},{"tokenIndex":8,"token":"9145b73a-d501-4327-8152-383b363c84e2"},{"token":"940c27f2-4df6-4bda-bcc9-0d84d6b4298a","tokenIndex":66},{"tokenIndex":79,"token":"94342e34-9836-48b8-b86b-387e5f5af35c"},{"tokenIndex":12,"token":"9825f17e-6410-411b-a838-8121763e20f0"},{"token":"99ffcf74-d231-4d4c-bfaf-144cc86f08d5","tokenIndex":81},{"token":"9a5c562f-db08-4bb1-aa98-b27c6f8bbfea","tokenIndex":89},{"tokenIndex":10,"token":"9a5e1f22-abd9-4e1d-88d1-9fe0316ab88e"},{"tokenIndex":69,"token":"9d870362-6a1a-44ca-a74f-5e74d610663d"},{"tokenIndex":98,"token":"a1cbb5e6-0084-456a-a233-1bcd91712f6f"},{"tokenIndex":14,"token":"a1f06115-84e2-45fe-a611-d771b9485066"},{"tokenIndex":19,"token":"a558f752-53bd-4bfb-8d70-7fcf50920ec9"},{"token":"a57ef0d8-752d-4fa1-9afb-39ce7c9ac914","tokenIndex":75},{"tokenIndex":1,"token":"a62dfd05-5224-4236-941b-d7c434620746"},{"tokenIndex":53,"token":"a7ab9d7a-6713-45e4-be0f-d746d9f7c018"},{"tokenIndex":82,"token":"ada20e0e-bf45-47b6-a8a6-86579399fdb4"},{"tokenIndex":88,"token":"af24511a-3688-48d2-bdce-b7c341a84ea2"},{"token":"b0fe5534-bca3-436e-ba91-2bcd9df2e0eb","tokenIndex":61},{"tokenIndex":39,"token":"b5ca548d-6429-4481-b6be-08f55e5a9178"},{"token":"b7c99f24-58aa-4be0-bb79-15c5b60cc191","tokenIndex":45},{"token":"bee89543-99a6-4525-92c2-00773ba8206c","tokenIndex":58}];
    const questionIndices = [10];
    const filteredTokens = tokens.filter(token => questionIndices.includes(token.tokenIndex));
    console.log(JSON.stringify(filteredTokens));
  })
});

describe('The getTokens function', function() {
  it('should get all tokens for a voter id', async() => {
    // arrange
    const tokens = [ 'token1', 'token2', 'token3' ];
    const signed = [ 'signed1', 'signed2', 'signed3' ];
    const blindingFactors = [ 'blinding1', 'blinding2', 'blinding3' ];
    await dbwrapper.insertVotingTokens('testVotingDeleteToken2', 'voter456', tokens, signed, blindingFactors);
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var readTokens = await dbwrapper.getVotingTokens('voter456');
    await timeout(DATASTORE_WAIT_TIME);

    // assert
    expect(readTokens.length).to.equal(3);
    expect(readTokens[0].tokenIndex).to.equal(0);
    expect(readTokens[1].tokenIndex).to.equal(1);
    expect(readTokens[2].tokenIndex).to.equal(2);
    expect(readTokens[0].token).to.equal('token1');
    expect(readTokens[1].token).to.equal('token2');
    expect(readTokens[2].token).to.equal('token3');
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

describe('The getPassword and setPassword function', function() {
  after(async() => {
    await deletePassword('foobar');
    await deletePassword('foobar2');
    await deletePassword('foobar3');
    await deletePassword('barfoo3');
  });
  it('should read and write a password', async() => {
    // arrange
    await dbwrapper.savePasswordHash('foobar', 'pass1234');
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var password = await dbwrapper.getPasswordHash('foobar');

    // assert
    expect(password).to.equal('pass1234');
  });

  it('should not fail with a bad id', async() => {
    // arrange
    await dbwrapper.savePasswordHash('foobar2', 'pass1234');
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var password = await dbwrapper.getPasswordHash('barfoo2');

    // assert
    assert(!password);
  });  

  it('should update the voting id', async() => {
    // arrange
    await dbwrapper.savePasswordHash('foobar3', 'pass1234');
    await timeout(DATASTORE_WAIT_TIME);
    await dbwrapper.updatePasswordHash('foobar3', 'barfoo3');
    await timeout(DATASTORE_WAIT_TIME);

    // act
    var password = await dbwrapper.getPasswordHash('barfoo3');

    // assert
    expect(password).to.equal('pass1234');
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

async function deletePassword(id) {
  const oldKey = datastore.key([TABLE_PASSWORDS, id]);
  const result = await datastore.get(oldKey);

  if(!result || !result[0] || result[0].length === 0) {
    return; // TODO log
  }

  await datastore.delete(oldKey);
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
