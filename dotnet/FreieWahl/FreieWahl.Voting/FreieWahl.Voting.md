<a name='assembly'></a>
# FreieWahl.Voting

## Contents

- [IChallengeStore](#T-FreieWahl-Voting-Registrations-IChallengeStore 'FreieWahl.Voting.Registrations.IChallengeStore')
  - [DeleteChallenge(registrationId)](#M-FreieWahl-Voting-Registrations-IChallengeStore-DeleteChallenge-System-String- 'FreieWahl.Voting.Registrations.IChallengeStore.DeleteChallenge(System.String)')
  - [DeleteChallenges(votingId)](#M-FreieWahl-Voting-Registrations-IChallengeStore-DeleteChallenges-System-String- 'FreieWahl.Voting.Registrations.IChallengeStore.DeleteChallenges(System.String)')
  - [GetChallenge(registrationId)](#M-FreieWahl-Voting-Registrations-IChallengeStore-GetChallenge-System-String- 'FreieWahl.Voting.Registrations.IChallengeStore.GetChallenge(System.String)')
  - [SetChallenge(challenge)](#M-FreieWahl-Voting-Registrations-IChallengeStore-SetChallenge-FreieWahl-Voting-Registrations-Challenge- 'FreieWahl.Voting.Registrations.IChallengeStore.SetChallenge(FreieWahl.Voting.Registrations.Challenge)')
- [IRegistrationStore](#T-FreieWahl-Voting-Registrations-IRegistrationStore 'FreieWahl.Voting.Registrations.IRegistrationStore')
  - [AddCompletedRegistration(completedRegistration)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-AddCompletedRegistration-FreieWahl-Voting-Registrations-CompletedRegistration- 'FreieWahl.Voting.Registrations.IRegistrationStore.AddCompletedRegistration(FreieWahl.Voting.Registrations.CompletedRegistration)')
  - [AddOpenRegistration(openRegistration)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-AddOpenRegistration-FreieWahl-Voting-Registrations-OpenRegistration- 'FreieWahl.Voting.Registrations.IRegistrationStore.AddOpenRegistration(FreieWahl.Voting.Registrations.OpenRegistration)')
  - [GetCompletedRegistrations(votingId)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-GetCompletedRegistrations-System-String- 'FreieWahl.Voting.Registrations.IRegistrationStore.GetCompletedRegistrations(System.String)')
  - [GetOpenRegistration(registrationStoreId)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-GetOpenRegistration-System-String- 'FreieWahl.Voting.Registrations.IRegistrationStore.GetOpenRegistration(System.String)')
  - [GetOpenRegistrationsForVoting(votingId)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-GetOpenRegistrationsForVoting-System-String- 'FreieWahl.Voting.Registrations.IRegistrationStore.GetOpenRegistrationsForVoting(System.String)')
  - [IsRegistrationUnique(dataSigneeId,votingId)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-IsRegistrationUnique-System-String,System-String- 'FreieWahl.Voting.Registrations.IRegistrationStore.IsRegistrationUnique(System.String,System.String)')
  - [RemoveOpenRegistration(id)](#M-FreieWahl-Voting-Registrations-IRegistrationStore-RemoveOpenRegistration-System-String- 'FreieWahl.Voting.Registrations.IRegistrationStore.RemoveOpenRegistration(System.String)')
- [IVotingResultStore](#T-FreieWahl-Voting-Storage-IVotingResultStore 'FreieWahl.Voting.Storage.IVotingResultStore')
  - [GetLastVote(votingId,questionIndex)](#M-FreieWahl-Voting-Storage-IVotingResultStore-GetLastVote-System-String,System-Int32- 'FreieWahl.Voting.Storage.IVotingResultStore.GetLastVote(System.String,System.Int32)')
  - [GetVotes(votingId)](#M-FreieWahl-Voting-Storage-IVotingResultStore-GetVotes-System-String- 'FreieWahl.Voting.Storage.IVotingResultStore.GetVotes(System.String)')
  - [GetVotes(votingId,questionIndex)](#M-FreieWahl-Voting-Storage-IVotingResultStore-GetVotes-System-String,System-Int32- 'FreieWahl.Voting.Storage.IVotingResultStore.GetVotes(System.String,System.Int32)')
  - [StoreVote(v,getBlockSignature,getGenesisSignature)](#M-FreieWahl-Voting-Storage-IVotingResultStore-StoreVote-FreieWahl-Voting-Models-Vote,System-Func{FreieWahl-Voting-Models-Vote,System-String},System-Func{System-Threading-Tasks-Task{System-String}}- 'FreieWahl.Voting.Storage.IVotingResultStore.StoreVote(FreieWahl.Voting.Models.Vote,System.Func{FreieWahl.Voting.Models.Vote,System.String},System.Func{System.Threading.Tasks.Task{System.String}})')
- [IVotingStore](#T-FreieWahl-Voting-Storage-IVotingStore 'FreieWahl.Voting.Storage.IVotingStore')

<a name='T-FreieWahl-Voting-Registrations-IChallengeStore'></a>
## IChallengeStore `type`

##### Namespace

FreieWahl.Voting.Registrations

##### Summary

Stores challenges required for authenticating registration requests to the tokenstore application

<a name='M-FreieWahl-Voting-Registrations-IChallengeStore-DeleteChallenge-System-String-'></a>
### DeleteChallenge(registrationId) `method`

##### Summary

deletes the challenge for a given registration id (e.g. when a registration is denied)

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |

<a name='M-FreieWahl-Voting-Registrations-IChallengeStore-DeleteChallenges-System-String-'></a>
### DeleteChallenges(votingId) `method`

##### Summary

deletes all open challenges for a voting (e.g. when the voting is deleted)

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of a voting |

<a name='M-FreieWahl-Voting-Registrations-IChallengeStore-GetChallenge-System-String-'></a>
### GetChallenge(registrationId) `method`

##### Summary

gets the challenge for a given registration

##### Returns

the challenge for the registration with the given id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |

<a name='M-FreieWahl-Voting-Registrations-IChallengeStore-SetChallenge-FreieWahl-Voting-Registrations-Challenge-'></a>
### SetChallenge(challenge) `method`

##### Summary

stores a given challenge

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| challenge | [FreieWahl.Voting.Registrations.Challenge](#T-FreieWahl-Voting-Registrations-Challenge 'FreieWahl.Voting.Registrations.Challenge') | a challenge |

<a name='T-FreieWahl-Voting-Registrations-IRegistrationStore'></a>
## IRegistrationStore `type`

##### Namespace

FreieWahl.Voting.Registrations

##### Summary

stores all open and completed registrations.

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-AddCompletedRegistration-FreieWahl-Voting-Registrations-CompletedRegistration-'></a>
### AddCompletedRegistration(completedRegistration) `method`

##### Summary

adds a completed registration

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| completedRegistration | [FreieWahl.Voting.Registrations.CompletedRegistration](#T-FreieWahl-Voting-Registrations-CompletedRegistration 'FreieWahl.Voting.Registrations.CompletedRegistration') | the completed registration (granted or denied) |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-AddOpenRegistration-FreieWahl-Voting-Registrations-OpenRegistration-'></a>
### AddOpenRegistration(openRegistration) `method`

##### Summary

adds an open registration

##### Returns

the future of this registration

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| openRegistration | [FreieWahl.Voting.Registrations.OpenRegistration](#T-FreieWahl-Voting-Registrations-OpenRegistration 'FreieWahl.Voting.Registrations.OpenRegistration') | the open registration |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-GetCompletedRegistrations-System-String-'></a>
### GetCompletedRegistrations(votingId) `method`

##### Summary

gets all completed registrations for a given voting id

##### Returns

a list of completed registrations (denied or granted)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-GetOpenRegistration-System-String-'></a>
### GetOpenRegistration(registrationStoreId) `method`

##### Summary

gets a single open registration

##### Returns

the registration with the given id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationStoreId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-GetOpenRegistrationsForVoting-System-String-'></a>
### GetOpenRegistrationsForVoting(votingId) `method`

##### Summary

gets all open registrations for a given voting

##### Returns

a list of open registrations

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-IsRegistrationUnique-System-String,System-String-'></a>
### IsRegistrationUnique(dataSigneeId,votingId) `method`

##### Summary

checks if someone with the given signee id has already registered for a given voting id - required to avoid double registrations

##### Returns

true, if there is no registration for the given signee id and voting id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| dataSigneeId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the signee id of the voter (e.g. mobile phone number or buergerkarten-number) |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |

<a name='M-FreieWahl-Voting-Registrations-IRegistrationStore-RemoveOpenRegistration-System-String-'></a>
### RemoveOpenRegistration(id) `method`

##### Summary

removes an open registration

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |

<a name='T-FreieWahl-Voting-Storage-IVotingResultStore'></a>
## IVotingResultStore `type`

##### Namespace

FreieWahl.Voting.Storage

##### Summary

stores votes (i.e. voting results) in a block chain like data structure

<a name='M-FreieWahl-Voting-Storage-IVotingResultStore-GetLastVote-System-String,System-Int32-'></a>
### GetLastVote(votingId,questionIndex) `method`

##### Summary

gets the last vote for a given voting with for the question with the given index

##### Returns

the last vote for the voting with the given id for the question with the given index

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |
| questionIndex | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | the question index |

<a name='M-FreieWahl-Voting-Storage-IVotingResultStore-GetVotes-System-String-'></a>
### GetVotes(votingId) `method`

##### Summary

gets all votes for a given voting id

##### Returns

a list for all votes for all questions with the given voting id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |

<a name='M-FreieWahl-Voting-Storage-IVotingResultStore-GetVotes-System-String,System-Int32-'></a>
### GetVotes(votingId,questionIndex) `method`

##### Summary

gets all votes for a given voting id and question index

##### Returns

a list for all votes for a given question in the voting with the given id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |
| questionIndex | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | the index of a question |

<a name='M-FreieWahl-Voting-Storage-IVotingResultStore-StoreVote-FreieWahl-Voting-Models-Vote,System-Func{FreieWahl-Voting-Models-Vote,System-String},System-Func{System-Threading-Tasks-Task{System-String}}-'></a>
### StoreVote(v,getBlockSignature,getGenesisSignature) `method`

##### Summary

stores a vote

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| v | [FreieWahl.Voting.Models.Vote](#T-FreieWahl-Voting-Models-Vote 'FreieWahl.Voting.Models.Vote') | the vote that should be stored |
| getBlockSignature | [System.Func{FreieWahl.Voting.Models.Vote,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{FreieWahl.Voting.Models.Vote,System.String}') | callback for getting the signature for a block |
| getGenesisSignature | [System.Func{System.Threading.Tasks.Task{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Threading.Tasks.Task{System.String}}') | callback for generating the genesis block signature |

<a name='T-FreieWahl-Voting-Storage-IVotingStore'></a>
## IVotingStore `type`

##### Namespace

FreieWahl.Voting.Storage

##### Summary

Stores votings and handles CRUD-operations for votings and question within these votings
