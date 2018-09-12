var chai = require('chai');
var chaiHttp = require('chai-http');
var server = require('./../app');

chai.use(chaiHttp);

describe('A call to saveRegistrationDetails', function() {
  it('should list ALL blobs on /blobs GET', function() {
    chai.request(server)
      .post('/saveRegistrationDetails')
      .send({
        id: '1234abcd',
        email: 'sergej.brin@alphabet.com',
        password: 'foobar42',
        tokenCount: 10,
        votingId: '12345'
      })
      .end(function(err, res) {
        res.should.have.status(200);
        done();
      });
  });
});
