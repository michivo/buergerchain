# Storing voting results

The voting results are stored in a blockchain-like data structure. A blockchain is created for each
question that was part of a voting. The genesis block of each of these blockchains holds a checksum
of the respective question, thus making sure that the question itself was not altered after the
voting process. For every vote that was cast, a block is added to the blockchain. Each block 
contains the following data:

* *Voting id* - the voting id uniquely identifies a voting
* *Question index* - the question index uniquely identifies a question within a voting
* *List of selected answer ids* - contains the answers selected by the voter, or no entries, if the voter abstained from voting
* *Previous block signature* - the signature of the previous block, i.e. the link between the blocks in the blockchain
* *Time stamp data* - a time stamp provided by a 3rd party time stamp server
* *Token* - the voting token that was used to cast the vote - this cannot be traced back to a voter, but each voter can verify that their vote was processed correctly using their voter id and password
* *Signed token* - the signed voting token, proving that the voting token was in fact valid

## Integrity verification

It lies in the nature of blockchains that any manipulation can easily be detected: If an attacker
deletes blocks, alters or adds blocks, the blockchain becomes **invalid**, or in other words, the
links connecting the blocks in the chain are broken.

Another type of attack would be to delete the entire blockchain and simply replace it with
a different sequence of blocks. In this project, this is prevented by adding a trusted time stamp 
provided by 3rd party time stamp servers to each block. The timestamps used in project buergerchain
are created using RFC3161 compliant time stamp authority servers run by Apple, Symantec, freetsa.org
and certum.pl .

When voting results are presented to the user, a basic integrity check is run over the voting
results, checking the integrity of the block chain. The user would immediately see if the voting
results have been tampered with.

## Implementation

The implementation of the blockchain logic can be found in classes [VotingResultManager](dotnet/FreieWahl/FreieWahl.Application/VotingResults/VotingResultManager.cs) and [VotingChainBuilder](dotnet/FreieWahl/FreieWahl.Application/VotingResults/VotingChainBuilder.cs).
