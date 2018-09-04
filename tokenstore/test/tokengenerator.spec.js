const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const sha256 = require('sha256');
const BigInteger = require('jsbn').BigInteger;
const NodeRSA = require('node-rsa');

const tokengenerator = require('./../tokengenerator');

const config = require('./../config.json');


describe('Blinding a message', function () {
  it('works', function () {
    const token = 'ba2b5e6e-cebe-4f78-b0d2-b4e2725810b3';
    const pwd = '123';
    const e = '10001';
    const n = '8f0f716a20a0bd9fea5c0f2bc8d4ca64b3622dff59b15ca32efb982d7487f8feee4764e9d7d6a08f26e8fc84688a3641299a0efd0475b5fc2fb47e15ca4abe8903b0ca8865c3a0494c7bb5d109fa66fe1ed350c2c333405d24912fdce810a18ddecc5ecc3ce6ab07a2c25139d2840fe79d43f465c06e1dd1ec4d83d938f2e9d981cfb37c215310aba96ae93ff2c45069dad9ccaacc340c3a1431dbd19eefc4f00e400c2fe0b5890defbfbbe5dc2e16e25e1271624c3b6001468f8f2029d92c2c30db26d341cbd53c68ac965a0ef7a833079a6d9874cb5763586dcd2795f0e5ae861bcfd512889abb69a8037824ce1869fe8cc8fc12ea13faf8c4bfe98a54ffb5';
    const r = '6b647a635c352cb19fedb4d4050e55626fa866976f99617cc499a15da5a5ba8073d2095497566f72d233722cf071fece161ced4efbe7579603926dd965e6d339';
    var blindedToken = tokengenerator.blindToken(token, pwd, n.toString(16), e.toString(16), r);
    expect(blindedToken.blinded).to.equal('79db383f6d8397df44a24690d1cd0fcd8a7ce7978565157a26273a6d827a625a1eee3768c1f85dabfe9fe177b66df251b4712b5bc8e9bdc3e09525b87867176c17e25730f287878a1a1ce507a87db4e971b536b4f6d50c26375ad5311528621f07ac7a569722a5f2d5f97c17ea4b62e99a94b45c3a44fd80307d2c8c05ab9e19beb04850ea5b98d3b1d456b71d02e0560286439d13e25c3c9be0659d4f9731cdadf72f1fefe670d3018a511166b823bec2ec89ef08945807abeec648aaed298d51c5c0fb72634546fe884c1e48e5c79c226b54972b5b7d7b86a1d3be380fb1eb9d6b2df38fc009eb58bb235fc6ea19bbd70d833b77afe3c91bad81d03fdf2246');
  })
});

describe('Unblinding a token', function() {
  it('works with real data', function() {
    var blindedSignedToken =  '115e5527c6f8505ce512ca81417ee42a276b61d8496b5021e4b974a2ba2b68f1ee67ab011790a49c8aaf8cc95a7097e26f23d1393c2dedfcad60018eacd8a356011e2fe85d761eed75b4fb5a5978161be84f40d55b3123c141ece65a2cd3ddfe22110d7088fa7cbb3786b842e428355c7dc321d091954b8dd1e74df447269083f763500aaf7f5af62b035cad9dca9ebe2f86ad8cd7f1a9fa9f68b195e5179d7fbb4d3203d062b9f9e45069e574dae417737a1cef4abb37008cf8fa7dd3fbfd663c872bbd2acdcdc690222f75445657de9c18df02068f18166159542771fb362b1af1f1c55e43956a3d75078364a993e72eabb02488530bed10df87401b6326cf';
    var r = '6b647a635c352cb19fedb4d4050e55626fa866976f99617cc499a15da5a5ba80d5b7ad0db71440ef934d3a4b1fadb176b656f27104f8f7e89a1ceb2e9244a9da';
    const n = '8f0f716a20a0bd9fea5c0f2bc8d4ca64b3622dff59b15ca32efb982d7487f8feee4764e9d7d6a08f26e8fc84688a3641299a0efd0475b5fc2fb47e15ca4abe8903b0ca8865c3a0494c7bb5d109fa66fe1ed350c2c333405d24912fdce810a18ddecc5ecc3ce6ab07a2c25139d2840fe79d43f465c06e1dd1ec4d83d938f2e9d981cfb37c215310aba96ae93ff2c45069dad9ccaacc340c3a1431dbd19eefc4f00e400c2fe0b5890defbfbbe5dc2e16e25e1271624c3b6001468f8f2029d92c2c30db26d341cbd53c68ac965a0ef7a833079a6d9874cb5763586dcd2795f0e5ae861bcfd512889abb69a8037824ce1869fe8cc8fc12ea13faf8c4bfe98a54ffb5';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, '123', n);
    const result = verify({
      unblinded: new BigInteger(unblindedToken, 16),
      N: new BigInteger( n, 16),
      E: new BigInteger("65537", 10),
      message: "ba2b5e6e-cebe-4f78-b0d2-b4e2725810b3"
    })

    expect(result).to.equal(true);
  }),
  it('fails with a wrong password', function() {
    var blindedSignedToken =  '115e5527c6f8505ce512ca81417ee42a276b61d8496b5021e4b974a2ba2b68f1ee67ab011790a49c8aaf8cc95a7097e26f23d1393c2dedfcad60018eacd8a356011e2fe85d761eed75b4fb5a5978161be84f40d55b3123c141ece65a2cd3ddfe22110d7088fa7cbb3786b842e428355c7dc321d091954b8dd1e74df447269083f763500aaf7f5af62b035cad9dca9ebe2f86ad8cd7f1a9fa9f68b195e5179d7fbb4d3203d062b9f9e45069e574dae417737a1cef4abb37008cf8fa7dd3fbfd663c872bbd2acdcdc690222f75445657de9c18df02068f18166159542771fb362b1af1f1c55e43956a3d75078364a993e72eabb02488530bed10df87401b6326cf';
    var r = '6b647a635c352cb19fedb4d4050e55626fa866976f99617cc499a15da5a5ba80d5b7ad0db71440ef934d3a4b1fadb176b656f27104f8f7e89a1ceb2e9244a9da';
    const n = '8f0f716a20a0bd9fea5c0f2bc8d4ca64b3622dff59b15ca32efb982d7487f8feee4764e9d7d6a08f26e8fc84688a3641299a0efd0475b5fc2fb47e15ca4abe8903b0ca8865c3a0494c7bb5d109fa66fe1ed350c2c333405d24912fdce810a18ddecc5ecc3ce6ab07a2c25139d2840fe79d43f465c06e1dd1ec4d83d938f2e9d981cfb37c215310aba96ae93ff2c45069dad9ccaacc340c3a1431dbd19eefc4f00e400c2fe0b5890defbfbbe5dc2e16e25e1271624c3b6001468f8f2029d92c2c30db26d341cbd53c68ac965a0ef7a833079a6d9874cb5763586dcd2795f0e5ae861bcfd512889abb69a8037824ce1869fe8cc8fc12ea13faf8c4bfe98a54ffb5';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, '133', n);
    const result = verify({
      unblinded: new BigInteger(unblindedToken, 16),
      N: new BigInteger( n, 16),
      E: new BigInteger("65537", 10),
      message: "ba2b5e6e-cebe-4f78-b0d2-b4e2725810b3"
    })

    expect(result).to.equal(false);
  }),
  it('fails with a wrong message', function() {
    var blindedSignedToken =  '115e5527c6f8505ce512ca81417ee42a276b61d8496b5021e4b974a2ba2b68f1ee67ab011790a49c8aaf8cc95a7097e26f23d1393c2dedfcad60018eacd8a356011e2fe85d761eed75b4fb5a5978161be84f40d55b3123c141ece65a2cd3ddfe22110d7088fa7cbb3786b842e428355c7dc321d091954b8dd1e74df447269083f763500aaf7f5af62b035cad9dca9ebe2f86ad8cd7f1a9fa9f68b195e5179d7fbb4d3203d062b9f9e45069e574dae417737a1cef4abb37008cf8fa7dd3fbfd663c872bbd2acdcdc690222f75445657de9c18df02068f18166159542771fb362b1af1f1c55e43956a3d75078364a993e72eabb02488530bed10df87401b6326cf';
    var r = '6b647a635c352cb19fedb4d4050e55626fa866976f99617cc499a15da5a5ba80d5b7ad0db71440ef934d3a4b1fadb176b656f27104f8f7e89a1ceb2e9244a9da';
    const n = '8f0f716a20a0bd9fea5c0f2bc8d4ca64b3622dff59b15ca32efb982d7487f8feee4764e9d7d6a08f26e8fc84688a3641299a0efd0475b5fc2fb47e15ca4abe8903b0ca8865c3a0494c7bb5d109fa66fe1ed350c2c333405d24912fdce810a18ddecc5ecc3ce6ab07a2c25139d2840fe79d43f465c06e1dd1ec4d83d938f2e9d981cfb37c215310aba96ae93ff2c45069dad9ccaacc340c3a1431dbd19eefc4f00e400c2fe0b5890defbfbbe5dc2e16e25e1271624c3b6001468f8f2029d92c2c30db26d341cbd53c68ac965a0ef7a833079a6d9874cb5763586dcd2795f0e5ae861bcfd512889abb69a8037824ce1869fe8cc8fc12ea13faf8c4bfe98a54ffb5';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, '123', n);
    const result = verify({
      unblinded: new BigInteger(unblindedToken, 16),
      N: new BigInteger( n, 16),
      E: new BigInteger("65537", 10),
      message: "ba2b5e6e-cebe-4f78-b0d2-b4e2725810b2"
    })

    expect(result).to.equal(false);
  })
});

function verify({ unblinded, message, E, N }) {
  unblinded = new BigInteger(unblinded.toString());
  const messageHash = messageToHashInt(message);
  const originalMsg = unblinded.modPow(E, N);
  const result = messageHash.equals(originalMsg);
  return result;
}

function messageToHashInt(message) {
  const messageHash = sha256(message);
  const messageBig = new BigInteger(messageHash, 16);
  return messageBig;
}

function unblind({ signed, key, r, N }) {

}
