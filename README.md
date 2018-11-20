# buergerchain

The buergerchain is a web-based voting platform. Its main focus is to provide a high level of security, in particual with regard to the authentication of voters, safe storage and anynomity. buergerchain supports authentication through the  "buergerkarte" (see www.buergerkarte.at), Austria’s official method for citiziens to provide a proof of identity. Further authentication methods are planned for future development, in particular 2-factor-authentication using sms & e-mail or technologies similar to Austria’s buergerkarte.

One installation of project buergerchain is available at www.freiewahl.eu. If you are a developer and want to contribute, please read the [developer's documentation](dev.md) and/or contact me.

## Terminology

### Voting:
A voting consists of one or more questions that should be answered by the voters within a defined timeframe. Exactly one voting administrator is responsible for such a voting.
The voting administrator creates a voting and configures it according to her needs, providing a title and a description for the voting and defining a time frame when users may vote. The administrator may then invite voters to register for the voting. The voting administrator will create a set of questions that are part of the voting. All voters may answer those questions after a successful registration within the defined time frame.

### Question:
As mentioned above, a voting consists of one or more questions. A voter needs to answer all questions or may abstain from voting. Currently, three types of questions are supported:
Decision questions: The voter needs to select one out of a list of options.
Multiple choice: The voter may select one or more options out of a given list. A maximum number of selectable options can be defined.
Ordering questions: The voters may select one or more options out of a given list and define an order of these options. A maximum number of selectable options can be defined.
The voting administrator can create and edit questions. Once she is done with editing a question, the administrator may unlock a question in order to make it available for voters. Once unlocked, the question cannot be edited anymore in order to guarantee that all voters answer exactly the same question.
Once the time frame for a voting is over, all questions in it are automatically locked and cannot be answered anymore. The administrator may also lock a question manually. Once a question is locked, its results are available to all voters and the voting administrator.

### Registration:
Each voter needs to register for a voting. The registration consists of four steps:
The voting administrator invites voters to participate in a voting. The invited voters get an e-mail with a link pointing to the registration site for a specific voting.
The voter visits the link that was sent to her in the invitation. The voter provides a proof of identity, e.g. using the Austrian buergerkarte. The voter chooses a password that will later be used when casting a vote.
The administrator verifies that the voter is actually eligible to vote and either grants or denies the voter’s registration.
The voter receives a unique link for the voting. She can only cast a vote with this unique link and the password chosen in step 2.
This process may sound a bit complicated but is necessary to ensure a high level of security and the voter’s anonymity.

### Voting administrator:
The voting administrator is a person who is responsible for creating, editing and configuring a voting. The administrator may be considered as the sole “owner” of a voting. Each administrator must create a user account and can only access her votings after logging in. The administrator is responsible for creating and maintaining a list of questions, for inviting voters and supervising the registration and voting process. The administrator can obviously view the results of a voting.

### Voter:
A voter is a person eligible to partake in a voting. The voter is invited by the voting administrator (usually by mail), registers for the voting and answers the questions the voting consists of. Each voter can view the results of all questions they voted on and may also see what they voted for.
