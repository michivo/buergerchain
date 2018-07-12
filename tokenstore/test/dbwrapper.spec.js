const sinon = require('sinon')
const chai = require('chai')

const expect = require('chai').expect
const assert = require('chai').assert

const dbwrapper = require('./../dbwrapper')

describe('The hello function', function () {
    it('says hello', function () {
        var hello = dbwrapper.hello();

        expect(hello).to.equal('hello dbwrapper!');
    })
})

describe('The addToken function', function () {
    it('adds a list of tokens', async () => {
        await dbwrapper.clearTokens('v12345');
        var tokens = [
            { index: 1, tokenId: 't1234', blindingFactor: 'b123456' },
            { index: 2, tokenId: 't4321', blindingFactor: 'b654321' }
        ];
        await dbwrapper.registerTokens('r12345987', 'v12345', 'michivo@gmail.com', tokens);

        var result = await dbwrapper.getRegisteredTokens('v12345')
        expect(result.length).to.equal(1);
        expect(result[0].registrationId).to.equal('r12345987');
        expect(result[0].votingId).to.equal('v12345');
        expect(result[0].tokens.length).to.equal(2);

        var token1 = result[0].tokens.find(e => e.index === 1);
        var token2 = result[0].tokens.find(e => e.index === 2);

        expect(token1.tokenId).to.equal('t1234');
        expect(token1.blindingFactor).to.equal('b123456');
        expect(token2.tokenId).to.equal('t4321');
        expect(token2.blindingFactor).to.equal('b654321');
    })
})