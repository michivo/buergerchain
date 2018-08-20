const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const sha256 = require('sha256');
const BigInteger = require('jsbn').BigInteger;

const tokengenerator = require('./../tokengenerator')

describe('Unblinding a blinded message', function () {
    it('returns the original message', function () {
        var token = tokengenerator.generateToken();
        var pwd = "foobar42";
        var pwdHash = sha256(token);
        var tokenHash = new BigInteger(sha256(token), 16);
        var blindedToken = tokengenerator.blindToken(token, pwdHash);
        console.log('blinded: ' + blindedToken.blinded.toString(16));
        console.log('r: ' + blindedToken.r.toString(16));
        var unblindedToken = tokengenerator.unblindToken(blindedToken.blinded, blindedToken.r, pwdHash);
        console.log('unblinded: ' + unblindedToken.toString(16));
        expect(unblindedToken).to.equal(tokenHash);
    })
})
