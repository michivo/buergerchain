const sgMail = require('@sendgrid/mail');
const privateConfig = require('./privateconfig.json');

sgMail.setApiKey(privateConfig.SG_API_KEY);

function sendInvitation(email, votingTitle, startDate, endDate, link) {
  const msg = {
    to: email,
    from: privateConfig.MAIL_FROM,
    subject: 'Erfolgreiche Registrierung für Abstimmung ' + votingTitle,
    text: `LiebeR WahlberechtigteR,<br /> Ihre Registrierung für die Abstimmung <strong>${votingTitle}</strong> war erfolgreich.` +
      `Sie können zwischen ${startDate} und ${endDate} unter <a href="${link}">${link}</a> an der Abstimmung teilnehmen.<br />` +
      'Wir freuen uns, wenn Sie von Ihrem Stimmrecht Gebrauch machen!'
  };

  sgMail.send(msg);
}

module.exports = {
  sendInvitation: sendInvitation
};
