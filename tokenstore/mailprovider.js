const sgMail = require('@sendgrid/mail');
const privateConfig = require('./privateconfig.json');

sgMail.setApiKey(privateConfig.SG_API_KEY);

function sendInvitation(email, voterId, votingId) {
  const msg = {
    to: email,
    from: privateConfig.MAIL_FROM,
    subject: 'Your registration to a voting was granted',
    text: 'great! your voter id is "' + voterId + '", your voting id is "' + votingId + '"'
  }

  sgMail.send(msg);
}

module.exports = {
  sendInvitation: sendInvitation
}
