# buergerchain

The buergerchain is a web-based voting platform. Its main focus is to provide a high level of security, in particual with regard to the authentication of voters, safe storage and anynomity. buergerchain supports authentication through the  "buergerkarte" (see www.buergerkarte.at), Austria’s official method for citiziens to provide a proof of identity. Further authentication methods are planned for future development, in particular 2-factor-authentication using sms & e-mail or technologies similar to Austria’s buergerkarte.

One installation of project buergerchain is available at www.freiewahl.eu. If you are a developer and want to contribute, please read the [developer's documentation](dev.md) and/or contact me.

## Terminology

### Voting
A voting consists of one or more questions that should be answered by the voters within a defined timeframe. Exactly one voting administrator is responsible for such a voting.

The voting administrator creates a voting and configures it according to her needs, providing a title and a description for the voting and defining a time frame when users may vote. The administrator may then invite voters to register for the voting. The voting administrator will create a set of questions that are part of the voting. All voters may answer those questions after a successful registration within the defined time frame.

### Question
As mentioned above, a voting consists of one or more questions. A voter needs to answer all questions or may abstain from voting. Currently, three types of questions are supported:

* Decision questions: The voter needs to select one out of a list of options.
* Multiple choice: The voter may select one or more options out of a given list. A maximum number of selectable options can be defined.
* Ordering questions: The voters may select one or more options out of a given list and define an order of these options. A maximum number of selectable options can be defined.

The voting administrator can create and edit questions. Once she is done with editing a question, the administrator may unlock a question in order to make it available for voters. Once unlocked, the question cannot be edited anymore in order to guarantee that all voters answer exactly the same question.
Once the time frame for a voting is over, all questions in it are automatically locked and cannot be answered anymore. The administrator may also lock a question manually. Once a question is locked, its results are available to all voters and the voting administrator.

### Registration
Each voter needs to register for a voting. The registration consists of four steps:
1. The voting administrator invites voters to participate in a voting. The invited voters get an e-mail with a link pointing to the registration site for a specific voting.
2. The voter visits the link that was sent to her in the invitation. The voter provides a proof of identity, e.g. using the Austrian buergerkarte. The voter chooses a password that will later be used when casting a vote.
3. The administrator verifies that the voter is actually eligible to vote and either grants or denies the voter’s registration. When it is granted, a list of *voting tokens* that are required when casting a vote.
4. The voter receives a unique link for the voting. She can only cast a vote with this unique link and the password chosen in step 2.
This process may sound a bit complicated but is necessary to ensure a high level of security and the voter’s anonymity.

### Voting administrator
The voting administrator is a person who is responsible for creating, editing and configuring a voting. The administrator may be considered as the sole “owner” of a voting. Each administrator must create a user account and can only access her votings after logging in. The administrator is responsible for creating and maintaining a list of questions, for inviting voters and supervising the registration and voting process. The administrator can obviously view the results of a voting.

### Voter
A voter is a person eligible to partake in a voting. The voter is invited by the voting administrator (usually by mail), registers for the voting and answers the questions the voting consists of. Each voter can view the results of all questions they voted on and may also see what they voted for.

### Voting tokens
When a voter registers for a voting and the registration is granted, a list of *voting tokens* is created. These tokens are unique, random character strings that are used when casting a vote. The voting tokens are used to make sure that only 

## Voting process
The voting process works as follows:
A voting is created by the voting administrator. A list of 100 RSA key pairs are created in the main application. The key pairs have indices running from 0 to 99, each key pair corresponds to one question in the voting - for this reason, no more than 100 questions may be part of one voting.
A voter is invited to a voting. The voter provides a proof of identity. When the voter registers for the voting, a unique registration id is created. This registration id is shared between the main application and the tokenstore. A list of 100 tokens and blinding factors is generated in the tokenstore and stored together with the registration id. Again, each of the 100 tokens may be used for one question in a specific voting. The blinding factors are applied to the tokens according to Chaum’s blinding scheme. The blinding factors are stored in the tokenstore, protected with the voter’s password.
The voting administrator grants the registration. When doing so, the main application fetches the 100 blinded tokens and signs them using the corresponding private key from the key pairs created in step 1.

The tokenstore stores the blinded, signed tokens and sends a mail to the voter containing a link with a unique voter id. This voter id has nothing to do with the aforementioned registration id and is not shared between the tokenstore and the main application, since it would allow the main application to trace back the voter’s identity.

The voter casts a vote, she answers one question. When sending the answer(s) from the voter’s browser to the main application, the voter’s browser fetches the token corresponding to the question from the tokenstore. The tokenstore gets the blinded, signed token and unblinds it. The client’s browser then sends the unblinded, signed token and the unblinded, unsigned token together with the answer(s) selected by the voter to the main application. The main application verifies the token’s signature, checks its validity for the current question and accepts the vote together with the token and the signed token.
Anyone can check if the token and signed token match using the public key created in step 1, the voter may even trace back that her vote was counted correctly.
