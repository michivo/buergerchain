const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const sha256 = require('sha256');
const BigInteger = require('jsbn').BigInteger;
const BlindSignature = require('blind-signatures');
const NodeRSA = require('node-rsa');

const tokengenerator = require('./../tokengenerator');

const config = require('./../config.json');

const PUBLIC_KEY_N = config.FREIEWAHL_PUBLICKEY_N;
const PUBLIC_KEY_E = config.FREIEWAHL_PUBLICKEY_E;

describe('Blinding a message', function () {
  it('works', function () {
    var token = tokengenerator.generateToken();
    var pwd = 'foobar42';
    var blindedToken = tokengenerator.blindToken(token, pwd);
  })
})

describe('Unblinding a token', function() {
  it('works', function() {
    var blindedSignedToken = new BigInteger( '94b920e79ce9c297bfa0a82cdfc599749e4a4713d9036702e5c7dd56184381e393674eca98da2e57fb4cff07731004e4cb7f22c7948228ad497c4102d6b89f11954b882affd2fa1cf8f33bd88c59e89ca2c27c8b0f3a9b224f3e6e4105b1f7e86ee0a29a2778a5571dab780b5be22fd94d25b69f00bf689e78845928c2228138e6244a7d6793b0fa80afa9542a6ac806ee3f7358f99f0b2c6fae3b9289988e0e8e85fa3c891565adf64c5c8bfcb3c451c0ebb55038c003d1240f40fe1f320c9833fb4367f8b83694602a664b7c285426049221e49fde6c1b4a78dd8f4359657004de3af88616d69d834c6fa9f0d5c1d407e2cd066e44c69438eac310f0e9df79', 16);
    var r = 'ce7cb577d0f15e0c15198a05d2985eb39dc66a8989ede8904d97a1007db42dfb2729189dd4089fdb7ac0849d45a6994a3079aa47e9b57b057dbb84ae1289294';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, 'foobar42');
    const result = BlindSignature.verify({
      unblinded: new BigInteger(unblindedToken, 16),
      N: PUBLIC_KEY_N,
      E: PUBLIC_KEY_E,
      message: "52870df1-e940-4739-b05f-cf40f7c443a1"
    })

    expect(result).to.equal(true);
  }),
  it('fails with a wrong password', function() {
    var blindedSignedToken = new BigInteger( '94b920e79ce9c297bfa0a82cdfc599749e4a4713d9036702e5c7dd56184381e393674eca98da2e57fb4cff07731004e4cb7f22c7948228ad497c4102d6b89f11954b882affd2fa1cf8f33bd88c59e89ca2c27c8b0f3a9b224f3e6e4105b1f7e86ee0a29a2778a5571dab780b5be22fd94d25b69f00bf689e78845928c2228138e6244a7d6793b0fa80afa9542a6ac806ee3f7358f99f0b2c6fae3b9289988e0e8e85fa3c891565adf64c5c8bfcb3c451c0ebb55038c003d1240f40fe1f320c9833fb4367f8b83694602a664b7c285426049221e49fde6c1b4a78dd8f4359657004de3af88616d69d834c6fa9f0d5c1d407e2cd066e44c69438eac310f0e9df79', 16);
    var r = 'ce7cb577d0f15e0c15198a05d2985eb39dc66a8989ede8904d97a1007db42dfb2729189dd4089fdb7ac0849d45a6994a3079aa47e9b57b057dbb84ae1289294';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, 'foobar43');
    const result = BlindSignature.verify({
      unblinded: unblindedToken,
      N: PUBLIC_KEY_N,
      E: PUBLIC_KEY_E,
      message: "52870df1-e940-4739-b05f-cf40f7c443a1"
    })

    expect(result).to.equal(false);
  }),
  it('fails with a wrong message', function() {
    var blindedSignedToken = new BigInteger( '94b920e79ce9c297bfa0a82cdfc599749e4a4713d9036702e5c7dd56184381e393674eca98da2e57fb4cff07731004e4cb7f22c7948228ad497c4102d6b89f11954b882affd2fa1cf8f33bd88c59e89ca2c27c8b0f3a9b224f3e6e4105b1f7e86ee0a29a2778a5571dab780b5be22fd94d25b69f00bf689e78845928c2228138e6244a7d6793b0fa80afa9542a6ac806ee3f7358f99f0b2c6fae3b9289988e0e8e85fa3c891565adf64c5c8bfcb3c451c0ebb55038c003d1240f40fe1f320c9833fb4367f8b83694602a664b7c285426049221e49fde6c1b4a78dd8f4359657004de3af88616d69d834c6fa9f0d5c1d407e2cd066e44c69438eac310f0e9df79', 16);
    var r = 'ce7cb577d0f15e0c15198a05d2985eb39dc66a8989ede8904d97a1007db42dfb2729189dd4089fdb7ac0849d45a6994a3079aa47e9b57b057dbb84ae1289294';
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, 'foobar42');
    const result = BlindSignature.verify({
      unblinded: new BigInteger(unblindedToken, 16),
      N: PUBLIC_KEY_N,
      E: PUBLIC_KEY_E,
      message: "52870df1-e940-4739-b05f-cf40f7c443a2"
    })

    expect(result).to.equal(false);
  })
})
