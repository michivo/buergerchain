function messageToHashInt(message) {
    var messageHash = sha256(message);
    var messageBig = new BigInteger(messageHash, 16);
    return messageBig;
}

function blind(message, N, E) {
    var messageHash = messageToHashInt(message);

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

        r = new BigInteger(rRaw, 16).mod(N);
        gcd = r.gcd(N);
    } while (
        !gcd.equals(bigOne) ||
            r.compareTo(N) >= 0 ||
            r.compareTo(bigOne) <= 0
    );
    var blinded = messageHash.multiply(r.modPow(E, N)).mod(N);
    return {
        blinded,
        r
    };
}

function unblind(signed, r, N) {
    return signed.multiply(r.modInverse(N)).mod(N);
}