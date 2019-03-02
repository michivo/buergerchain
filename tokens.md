## Voting tokens

A major challenge in project buergerchain was to make sure that all votes

* are anonymous, thus cannot be traced back to the voter
* a voter can verify that their votes were counted correctly
* were cast by voters actually eligible for voting

The system implemented in project buergerchain uses [RSA blinding](https://en.wikipedia.org/wiki/Blinding_(cryptography)), a commonly used technique to allow for making sure only eligible voters can cast a vote whilst maintaining their anonymity. The basic principles of the implementation are described here in more detail:
