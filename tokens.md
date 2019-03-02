## Voting tokens

A major challenge in project buergerchain was to make sure that all votes

* are anonymous, thus cannot be traced back to the voter
* a voter can verify that their votes were counted correctly
* were cast by voters actually eligible for voting

The system implemented in project buergerchain uses [RSA blinding](https://en.wikipedia.org/wiki/Blinding_(cryptography)), a commonly used technique to allow for making sure only eligible voters can cast a vote whilst maintaining their anonymity. The basic principles of the implementation are described here in more detail:

1. When a new voting is created, a list of 100 random 2048-bit RSA key pairs are created in the **main application** (which is implemented in C#)
1. When a voter registers for a voting and is granted to be eligible for voting, a list of 100 random *voting tokens* associated with the voter are created in the **tokenstore** (which is implemented in Node.js). 
1. These *voting tokens* are then blinded with a key generated using the voter's password and the voter's id. 
1. These *blinded voting tokens* are sent to the **main application** where they are blindly signed with the private keys of the 100 key pairs created in step 1, thus creating a list of 100 *blinded, signed voting tokens*. These *blinded, signed voting tokens* are stored together with the original *voting tokens* in the **tokenstore**.
1. When a voter casts a vote for a question (questions have indices 1-100, no voting may contain more than 100 questions), the corresponding *blinded, signed token* is unblinded, thus creating a *signed token*. The voter fetches the *signed token* together with the original *voting token* and sends those to the **main application**, where they are stored as described in the [storage documentation](storage.md). 

This way, the voters can prove that they are eligible to vote, maintaining anonymity at the same time. This also explains, why a voter's password can neither be recovered nor changed - all the voter's voting tokens are blinded using a password that was generated utilizing the voter's password. Once the password is lost, these voting tokens cannot be unblinded, thus rendering them useless.
