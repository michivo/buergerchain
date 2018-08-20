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

function blindToken(token, passwordHash) {
  var message = messageToHashInt(token);
  var pwdInt = new BigInteger(passwordHash, 16);
  var bigOne = new BigInteger('1');
  var gcd;
  var r;
  var rRaw;
  do {
      var randR = secureRandom(64);
      rRaw = "";
      for (var i = 0; i < 64; i++) {
          if (randR[i] < 16)
              rRaw += "0" + randR[i].toString(16);
          else
              rRaw += randR[i].toString(16);
      }

      r = new BigInteger(rRaw, 16).mod(PUBLIC_KEY_N);
      gcd = r.gcd(PUBLIC_KEY_N);
  } while (
      !gcd.equals(bigOne) ||
          r.compareTo(PUBLIC_KEY_N) >= 0 ||
          r.compareTo(bigOne) <= 0
  );
  var blinded = message.multiply(r.modPow(PUBLIC_KEY_E, PUBLIC_KEY_N)).mod(PUBLIC_KEY_N);
  r = r.xor(pwdInt);

  return {
      blinded,
      r
  };
}

function unblindToken(message, r, passwordHash) {
  var pwdInt = new BigInteger(passwordHash, 16);
  r = r.xor(pwdInt);
  return message.multiply(r.modInverse(PUBLIC_KEY_N)).mod(PUBLIC_KEY_N);
}


module.exports = {
  generateToken: generateToken,
  blindToken: blindToken,
  unblindToken: unblindToken
}
