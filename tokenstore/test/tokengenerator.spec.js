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
    var pwd = "foobar42";
    var pwdHash = sha256(pwd);
    var blindedToken = tokengenerator.blindToken(token, pwdHash);
    console.log('token:' + token);
    console.log('blinded: ' + blindedToken.blinded.toString(16));
    console.log('r: ' + blindedToken.r.toString(16));
  })
})

describe('Unblinding a token', function() {
  it('works', function() {
    var blindedSignedToken = new BigInteger( '51f6119ed5830706b430fc61f9e8f2263a85a9f36c97e9c54613b886fbb0c45d06cfddf1542c7228d29d7309a0b29db7b6ab8d6cb8eaabbddbca25b0eaccac5adb5e05c1836c408f93e264807fbaeb7a47c6a1c53dce98f522939439a38ab98f8929909b680463e4c1f7d16c8949aef950f9a75cee6ff7eac675f5b06a0abe38b792ee05cf1b130a04d4470e380fb7ba7152b72d116a1ba96de52f1027ce86ee893720266bfff3dcaac9ddc1edfe120ae6d080b74146da7c6c4bd7d6eb13835c8ba465c1a01573b1a8cf3b949e7e3f161b79a4853957f94649df40a56709120943879f130c3c735fca7383fd206b760464c22dad583dc05ac5bccec8766e57e', 16);
    var r= new BigInteger('4dfd92787756fc9971ed17d300a19e8450c926757328ec8bd6ea4eee585d2fa1b4db4467ed98eaa0ef183f869e280f61f6f6fb939ab7f312605a57f1a932413d', 16);
    var unblindedToken = tokengenerator.unblindToken(blindedSignedToken, r, 'foobar42');
    const result = BlindSignature.verify({
      unblinded: unblindedToken,
      N: PUBLIC_KEY_N,
      E: PUBLIC_KEY_E,
      message: "0bea3d39-f881-4e32-974e-7a33735e00fa"
    })
    console.log('unblinded: ' + unblindedToken.toString(16));
    expect(result).to.equal(true);
  })
})

describe('Playing with Alice and Bob', function() {
  it('is fun', function() {
    const bobkey = new NodeRSA();
    bobkey.importKey("-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEAss70Mhgyh1mWaNtFw6k07atjidVfPvfR6oxsJgVcxrWTAYhS\n5AFOQbppf2JNN8ddkHJftwdLdlN297vDRl9w/IrAbU52ttvRr6UveEx1/d/s9J6k\n5Kq1hTWpkA66TZY9cZCzBfSvUWruxHCRTlCr0YI8Epm+fLfduLJa5sfxrJHBkk+W\nakeIyaPqjqJxj0Iv7YRpTj07KF7PcGNTkAdW+7BKG8zTYfDeht5NEC/V8vgbCa78\nhloscV/NIfb/4n7sAAnxbuRGNbliBkQitYm/LR5opdoLpGUE6jTxPBTXPe5DlXDM\nYM7fv9oR18O/OLvPMsNUN/mZ/s/C37/C72qUtwIDAQABAoIBAAK7KH2vYu4wTxzQ\n5JLlfbp3mLCdQrQqgtlLRcea41zhnxox49o5ruFQIJZigP1uHR68sHuSL/PhuHp2\nMrhbctVYpTHGNgf6+YvuQPhcapzzE6J03d3kQZuEQ0/A+dV/iva2GBXqM9dRg84a\nTg3dK5Kqo5JBKOiswkU07DCEM0vIc42Lx3aK284ZolUoYlJbJZv/PqLzcZxd/PPm\nVgePJTpAy3RrQN3YtdmshVg4qCrCtDfX7d4l73ENlE3XTVz36QSPHI62qp77wf7I\nuIGoEnl7WGy94UddVxD/xW6SwJ0LfdIwVedf+BnQdknMM18oGwa7Rg0FhD5s+BmY\nhdVfIZECgYEA9ErS8THariVpfvVaOgQ41SF371JksUWmzqSH8kGDnzVXdYCluV71\nON0g/bHoWh49j2ZRJELZynYku63ONcBkp8sfM4BD3hTj/T8/U5N8CqUh0jCJw0B7\niQWcQ0Fm4b4q8UlJOeDkbkXGG3G/poHtq+xi22r+ONRsP8UR1Xdf6O8CgYEAu2C4\nfLsHPBczZ49habxGfIS7wUlQ2bnm+m5vutgPaKXcnQ4OeiVcZV96jljogNbOwTRV\n/ZUiX67CAoNZVCp45TSgzVJPQBDha0nG2D5LDOCFLdYpqP/B/TZ54Uu0DJFh6s2F\naB7D9cCE+agg88uneT8jqQ+WCXXHX6rCiHoYwLkCgYEAyxliIrDGFD56ZNjq+I0G\nCvvWUJv5pwA3XFmhxKD/IuAgJEqefW0bBvmhMgo1GKdHmu7/ytvhYdezVm17oWig\nxnezKwgaZIqNucBZj8xwNhFv+uXrwu7bReHqNmgrdsa5wPyi6oG0qJFN0QdSxMYE\nqQjQb4eWb/z7OlFHMGgczvUCgYBq/a454l0eLa03a8JWqp+gx/WhRyi4OZMu2dJI\nYMhjm5ldwEH58s1QQPVsxE12C7Gg1i5njjlDYzj6UF+4VEwVrDhJJL+FuF3OciDt\nJpyZ7LV+17OQAQGWgP2U7DIRnw3HEbUkH7UK5PPIzfyK2HV3INtO1Ex6eFrwQEO1\nw+nQWQKBgQCuCzMFA2JKDPE5W6Kr11Po4GF+ucC0WZls18uC5+vTKKVHevTcazPY\nAkU5y1zRY3ZY+8pOZqyWvp0zRzooG9ptzR+V+byPJSfyafeBoj0A7B3Gf/SqxVJF\nXWpyUp0ln/id2ThI8JB5gQaIsqavGeaXKv0VvFO+LUVuS+Rm4b4LMw==\n-----END RSA PRIVATE KEY-----");
    const Bob = {
      key: bobkey, // b: key-length
      blinded: null,
      unblinded: null,
      message: null,
    };

    console.log("D", bobkey.keyPair.d.toString(10));

    const Alice = {
      message: tokengenerator.generateToken(),
      N: null,
      E: null,
      r: null,
      signed: null,
      unblinded: null,
    };

    // Alice wants Bob to sign a message without revealing it's contents.
    // Bob can later verify he did sign the message

    console.log('Message:', Alice.message);

    // Alice gets N and E variables from Bob's key
    Alice.N = Bob.key.keyPair.n.toString();
    Alice.E = Bob.key.keyPair.e.toString();

    const { blinded, r } = BlindSignature.blind({
      message: Alice.message,
      N: Alice.N,
      E: Alice.E,
    }); // Alice blinds message
    Alice.r = r;

    // Alice sends blinded to Bob
    Bob.blinded = blinded;
    console.log('Blinded: ', blinded.toString(10));

    const signed = BlindSignature.sign({
      blinded: Bob.blinded,
      key: Bob.key,
    }); // Bob signs blinded message
    var signed2 = bobkey.sign(blinded);
    var signed2int = new BigInteger(signed2);
    // Bob sends signed to Alice
    Alice.signed = signed;
    console.log('Signed: ', signed.toString(10));
    console.log('Signed2: ', signed2int.toString());
    const unblinded = BlindSignature.unblind({
      signed: Alice.signed,
      N: Alice.N,
      r: Alice.r,
    }); // Alice unblinds
    Alice.unblinded = unblinded;
    console.log('Unblinded: ', unblinded.toString(10));
    // Alice verifies
    const result = BlindSignature.verify({
      unblinded: Alice.unblinded,
      N: Alice.N,
      E: Alice.E,
      message: Alice.message,
    });
    if (result) {
      console.log('Alice: Signatures verify!');
    } else {
      console.log('Alice: Invalid signature');
    }

    // Alice sends Bob unblinded signature and original message
    Bob.unblinded = Alice.unblinded;
    Bob.message = Alice.message;

    // Bob verifies
    const result2 = BlindSignature.verify2({
      unblinded: Bob.unblinded,
      key: Bob.key,
      message: Bob.message,
    });
    if (result2) {
      console.log('Bob: Signatures verify!');
    } else {
      console.log('Bob: Invalid signature');
    }
  })
})
