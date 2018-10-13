const secureRandom = require('secure-random');
const BigInteger = require('jsbn').BigInteger;
const uuidv4 = require('uuid/v4');
const sha256 = require('sha256');

const bigOne = new BigInteger('1');

function generateToken() {
  return uuidv4();
}

function messageToHashInt(message) {
  const messageHash = sha256(message);
  const messageBig = new BigInteger(messageHash, 16);
  return messageBig;
}

function blindToken(token, password, n, e, r) {
  const messageHash = messageToHashInt(token);
  const nBig = new BigInteger(n, 16);
  const eBig = new BigInteger(e, 16);

  const rBig = new BigInteger(r, 16) || getR(nBig);

  const blinded = messageHash.multiply(rBig.modPow(eBig, nBig)).mod(nBig);
  return {
    blinded: blinded.toString(16),
    r: rBig.xor(messageToHashInt(password)).toString(16)
  };
}

function getR(nBig) {
  let r;
  let gcd;
  do {
    r = new BigInteger(secureRandom(64)).mod(nBig);
    gcd = r.gcd(nBig);
  } while (
    !gcd.equals(bigOne) ||
    r.compareTo(nBig) >= 0 ||
    r.compareTo(bigOne) <= 0
  );

  return r;
}

function unblindToken(message, r, password, n) {
  const rBig = new BigInteger(r, 16).xor(messageToHashInt(password));
  const nBig = new BigInteger(n, 16);
  const signedBig = new BigInteger(message, 16);
  // todo check for division by 0?

  const unblinded = signedBig.multiply(rBig.modInverse(nBig)).mod(nBig);
  return unblinded.toString(16);
}

module.exports = {
  generateToken: generateToken,
  blindToken: blindToken,
  unblindToken: unblindToken
};
