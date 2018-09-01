const BlindSignature = require('blind-signatures');
const secureRandom = require('secure-random');
const NodeRSA = require('node-rsa');
const BigInteger = require('jsbn').BigInteger;
const config = require('./config.json');
const uuidv4 = require('uuid/v4');
const sha256 = require('sha256');

function generateToken() {
  return uuidv4();
}

function messageToHashInt(message) {
  const messageHash = sha256(message);
  const messageBig = new BigInteger(messageHash, 16);
  return messageBig;
}

function blindToken(token, password, n, e) {
  const result = BlindSignature.blind({
    message: token,
    N: n,
    E: e
  });

  return {
    r: result.r.xor(messageToHashInt(password)).toString(16),
    blinded: result.blinded.toString(16)
  }
}

function unblindToken(message, r, password, n) {
  r = new BigInteger(r, 16).xor(messageToHashInt(password));
  console.log('unpassworded r ' + r.toString(16));
  return BlindSignature.unblind({
    signed: message,
    N: n,
    r: r
  }).toString(16);
}


module.exports = {
  generateToken: generateToken,
  blindToken: blindToken,
  unblindToken: unblindToken
}
