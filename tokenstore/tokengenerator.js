const BlindSignature = require('blind-signatures');
const secureRandom = require('secure-random');
const NodeRSA = require('node-rsa');
const BigInteger = require('jsbn').BigInteger;
const config = require('./config.json');
const uuidv4 = require('uuid/v4');
const sha256 = require('sha256');

const PUBLIC_KEY_N = new BigInteger(config.FREIEWAHL_PUBLICKEY_N);
const PUBLIC_KEY_E = new BigInteger(config.FREIEWAHL_PUBLICKEY_E);

function generateToken() {
  return uuidv4();
}

function messageToHashInt(message) {
  var messageHash = sha256(message);
  var messageBig = new BigInteger(messageHash, 16);
  return messageBig;
}

function blindToken(token, password) {
  var result = BlindSignature.blind({
    message: token,
    N: PUBLIC_KEY_N,
    E: PUBLIC_KEY_E
  });
  
  return {
    r: result.r.xor(messageToHashInt(password)).toString(16),
    blinded: result.blinded.toString(16)
  }
}

function unblindToken(message, r, password) {
  r = r.xor(messageToHashInt(password));
  return BlindSignature.unblind({
    signed: message,
    N: PUBLIC_KEY_N,
    r: r
  }).toString(16);
}


module.exports = {
  generateToken: generateToken,
  blindToken: blindToken,
  unblindToken: unblindToken
}
