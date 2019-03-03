<a name='assembly'></a>
# FreieWahl.Application

## Contents

- [IAuthenticationManager](#T-FreieWahl-Application-Authentication-IAuthenticationManager 'FreieWahl.Application.Authentication.IAuthenticationManager')
  - [IsAuthorized(userId,votingId,operation)](#M-FreieWahl-Application-Authentication-IAuthenticationManager-IsAuthorized-FreieWahl-Security-UserHandling-UserInformation,System-String,FreieWahl-Application-Authentication-Operation- 'FreieWahl.Application.Authentication.IAuthenticationManager.IsAuthorized(FreieWahl.Security.UserHandling.UserInformation,System.String,FreieWahl.Application.Authentication.Operation)')
- [IAuthorizationHandler](#T-FreieWahl-Application-Authentication-IAuthorizationHandler 'FreieWahl.Application.Authentication.IAuthorizationHandler')
  - [CheckAuthorization(authToken,votingId,operation)](#M-FreieWahl-Application-Authentication-IAuthorizationHandler-CheckAuthorization-System-String,FreieWahl-Application-Authentication-Operation,System-String- 'FreieWahl.Application.Authentication.IAuthorizationHandler.CheckAuthorization(System.String,FreieWahl.Application.Authentication.Operation,System.String)')
- [IChallengeHandler](#T-FreieWahl-Application-Registrations-IChallengeHandler 'FreieWahl.Application.Registrations.IChallengeHandler')
  - [CreateChallenge(recipientName,recipientAddress,voting,registrationId,challengeType)](#M-FreieWahl-Application-Registrations-IChallengeHandler-CreateChallenge-System-String,System-String,FreieWahl-Voting-Models-StandardVoting,System-String,FreieWahl-Voting-Registrations-ChallengeType- 'FreieWahl.Application.Registrations.IChallengeHandler.CreateChallenge(System.String,System.String,FreieWahl.Voting.Models.StandardVoting,System.String,FreieWahl.Voting.Registrations.ChallengeType)')
  - [GetChallengeForRegistration(registrationId)](#M-FreieWahl-Application-Registrations-IChallengeHandler-GetChallengeForRegistration-System-String- 'FreieWahl.Application.Registrations.IChallengeHandler.GetChallengeForRegistration(System.String)')
- [IChallengeService](#T-FreieWahl-Application-Registrations-IChallengeService 'FreieWahl.Application.Registrations.IChallengeService')
  - [SupportedChallengeType](#P-FreieWahl-Application-Registrations-IChallengeService-SupportedChallengeType 'FreieWahl.Application.Registrations.IChallengeService.SupportedChallengeType')
  - [GetStandardizedRecipient(recipient)](#M-FreieWahl-Application-Registrations-IChallengeService-GetStandardizedRecipient-System-String- 'FreieWahl.Application.Registrations.IChallengeService.GetStandardizedRecipient(System.String)')
  - [SendChallenge(recipient,votingName,challenge,votingId)](#M-FreieWahl-Application-Registrations-IChallengeService-SendChallenge-System-String,System-String,System-String,System-String- 'FreieWahl.Application.Registrations.IChallengeService.SendChallenge(System.String,System.String,System.String,System.String)')
- [IRegistrationHandler](#T-FreieWahl-Application-Registrations-IRegistrationHandler 'FreieWahl.Application.Registrations.IRegistrationHandler')
  - [DenyRegistration(registrationId,userId)](#M-FreieWahl-Application-Registrations-IRegistrationHandler-DenyRegistration-System-String,System-String- 'FreieWahl.Application.Registrations.IRegistrationHandler.DenyRegistration(System.String,System.String)')
  - [GrantRegistration(registrationId,userId,votingUrl,utcOffset,timezoneName)](#M-FreieWahl-Application-Registrations-IRegistrationHandler-GrantRegistration-System-String,System-String,System-String,System-TimeSpan,System-String- 'FreieWahl.Application.Registrations.IRegistrationHandler.GrantRegistration(System.String,System.String,System.String,System.TimeSpan,System.String)')
- [IRemoteTokenStore](#T-FreieWahl-Application-Registrations-IRemoteTokenStore 'FreieWahl.Application.Registrations.IRemoteTokenStore')
  - [GetChallenge(registrationId)](#M-FreieWahl-Application-Registrations-IRemoteTokenStore-GetChallenge-System-String- 'FreieWahl.Application.Registrations.IRemoteTokenStore.GetChallenge(System.String)')
  - [GrantRegistration(registrationStoreId,voting,signedChallengeString,signedTokens,votingUrl,utcOffset,timezoneName)](#M-FreieWahl-Application-Registrations-IRemoteTokenStore-GrantRegistration-System-String,FreieWahl-Voting-Models-StandardVoting,System-String,System-Collections-Generic-List{System-String},System-String,System-TimeSpan,System-String- 'FreieWahl.Application.Registrations.IRemoteTokenStore.GrantRegistration(System.String,FreieWahl.Voting.Models.StandardVoting,System.String,System.Collections.Generic.List{System.String},System.String,System.TimeSpan,System.String)')
  - [InsertPublicKeys(votingId,publicKeys)](#M-FreieWahl-Application-Registrations-IRemoteTokenStore-InsertPublicKeys-System-String,System-Collections-Generic-IEnumerable{Org-BouncyCastle-Crypto-Parameters-RsaKeyParameters}- 'FreieWahl.Application.Registrations.IRemoteTokenStore.InsertPublicKeys(System.String,System.Collections.Generic.IEnumerable{Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters})')
- [ISessionCookieProvider](#T-FreieWahl-Application-Authentication-ISessionCookieProvider 'FreieWahl.Application.Authentication.ISessionCookieProvider')
  - [CreateSessionCookie(idToken)](#M-FreieWahl-Application-Authentication-ISessionCookieProvider-CreateSessionCookie-System-String- 'FreieWahl.Application.Authentication.ISessionCookieProvider.CreateSessionCookie(System.String)')
- [ITracker](#T-FreieWahl-Application-Tracking-ITracker 'FreieWahl.Application.Tracking.ITracker')
  - [Track(path,clientAddress,userAgent)](#M-FreieWahl-Application-Tracking-ITracker-Track-System-String,System-String,System-String- 'FreieWahl.Application.Tracking.ITracker.Track(System.String,System.String,System.String)')
  - [TrackSpending(votingId,cost)](#M-FreieWahl-Application-Tracking-ITracker-TrackSpending-System-String,System-String- 'FreieWahl.Application.Tracking.ITracker.TrackSpending(System.String,System.String)')
- [IVotingChainBuilder](#T-FreieWahl-Application-VotingResults-IVotingChainBuilder 'FreieWahl.Application.VotingResults.IVotingChainBuilder')
  - [CheckChain(q,votes)](#M-FreieWahl-Application-VotingResults-IVotingChainBuilder-CheckChain-FreieWahl-Voting-Models-Question,System-Collections-Generic-IReadOnlyList{FreieWahl-Voting-Models-Vote}- 'FreieWahl.Application.VotingResults.IVotingChainBuilder.CheckChain(FreieWahl.Voting.Models.Question,System.Collections.Generic.IReadOnlyList{FreieWahl.Voting.Models.Vote})')
  - [GetGenesisValue(q)](#M-FreieWahl-Application-VotingResults-IVotingChainBuilder-GetGenesisValue-FreieWahl-Voting-Models-Question- 'FreieWahl.Application.VotingResults.IVotingChainBuilder.GetGenesisValue(FreieWahl.Voting.Models.Question)')
  - [GetSignature(v)](#M-FreieWahl-Application-VotingResults-IVotingChainBuilder-GetSignature-FreieWahl-Voting-Models-Vote- 'FreieWahl.Application.VotingResults.IVotingChainBuilder.GetSignature(FreieWahl.Voting.Models.Vote)')
- [IVotingResultManager](#T-FreieWahl-Application-VotingResults-IVotingResultManager 'FreieWahl.Application.VotingResults.IVotingResultManager')
  - [GetResults(votingId,tokens)](#M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String,System-String[]- 'FreieWahl.Application.VotingResults.IVotingResultManager.GetResults(System.String,System.String[])')
  - [GetResults(votingId)](#M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String- 'FreieWahl.Application.VotingResults.IVotingResultManager.GetResults(System.String)')
  - [GetResults(votingId,questionIndex)](#M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String,System-Int32- 'FreieWahl.Application.VotingResults.IVotingResultManager.GetResults(System.String,System.Int32)')
  - [StoreVote(votingId,questionIndex,answers,token,signedToken)](#M-FreieWahl-Application-VotingResults-IVotingResultManager-StoreVote-System-String,System-Int32,System-Collections-Generic-List{System-String},System-String,System-String- 'FreieWahl.Application.VotingResults.IVotingResultManager.StoreVote(System.String,System.Int32,System.Collections.Generic.List{System.String},System.String,System.String)')

<a name='T-FreieWahl-Application-Authentication-IAuthenticationManager'></a>
## IAuthenticationManager `type`

##### Namespace

FreieWahl.Application.Authentication

##### Summary

Checks if a user is authorized to perform a certain action. There is only a pretty simple implementation,
since there is a pretty simple user system implemented at the moment.

<a name='M-FreieWahl-Application-Authentication-IAuthenticationManager-IsAuthorized-FreieWahl-Security-UserHandling-UserInformation,System-String,FreieWahl-Application-Authentication-Operation-'></a>
### IsAuthorized(userId,votingId,operation) `method`

##### Summary

Checks if a user is authorized to perform a certain action

##### Returns

result of the authorization (granted or not)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| userId | [FreieWahl.Security.UserHandling.UserInformation](#T-FreieWahl-Security-UserHandling-UserInformation 'FreieWahl.Security.UserHandling.UserInformation') | the user's id |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of voting that is affected by this operation |
| operation | [FreieWahl.Application.Authentication.Operation](#T-FreieWahl-Application-Authentication-Operation 'FreieWahl.Application.Authentication.Operation') | the operation that is about to be performed |

<a name='T-FreieWahl-Application-Authentication-IAuthorizationHandler'></a>
## IAuthorizationHandler `type`

##### Namespace

FreieWahl.Application.Authentication

##### Summary

Checks if a user providing the given authorization token is allowed to perform a certain operation

<a name='M-FreieWahl-Application-Authentication-IAuthorizationHandler-CheckAuthorization-System-String,FreieWahl-Application-Authentication-Operation,System-String-'></a>
### CheckAuthorization(authToken,votingId,operation) `method`

##### Summary

Checks if a user is authorized to perform a certain action

##### Returns

result of the authorization (granted or not)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| authToken | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the authToken provided by a user. This can be mapped to a user |
| votingId | [FreieWahl.Application.Authentication.Operation](#T-FreieWahl-Application-Authentication-Operation 'FreieWahl.Application.Authentication.Operation') | the id of voting that is affected by this operation |
| operation | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the operation that is about to be performed |

<a name='T-FreieWahl-Application-Registrations-IChallengeHandler'></a>
## IChallengeHandler `type`

##### Namespace

FreieWahl.Application.Registrations

##### Summary

Creates a challenge for authentication via mobile phones/SMS

<a name='M-FreieWahl-Application-Registrations-IChallengeHandler-CreateChallenge-System-String,System-String,FreieWahl-Voting-Models-StandardVoting,System-String,FreieWahl-Voting-Registrations-ChallengeType-'></a>
### CreateChallenge(recipientName,recipientAddress,voting,registrationId,challengeType) `method`

##### Summary

creates, sends and stores a new challenge

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| recipientName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the recipient's name |
| recipientAddress | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the address (mail address, e-mail address, or phone number) of the recipient |
| voting | [FreieWahl.Voting.Models.StandardVoting](#T-FreieWahl-Voting-Models-StandardVoting 'FreieWahl.Voting.Models.StandardVoting') | the voting the voter has registered for |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |
| challengeType | [FreieWahl.Voting.Registrations.ChallengeType](#T-FreieWahl-Voting-Registrations-ChallengeType 'FreieWahl.Voting.Registrations.ChallengeType') | the challenge type (mail, e-mail, sms) |

<a name='M-FreieWahl-Application-Registrations-IChallengeHandler-GetChallengeForRegistration-System-String-'></a>
### GetChallengeForRegistration(registrationId) `method`

##### Summary

gets the challenge for a given registration id. this is called when the user provides a reply for the challenge

##### Returns

the challenge sent out to the voter with the given registration id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration id |

<a name='T-FreieWahl-Application-Registrations-IChallengeService'></a>
## IChallengeService `type`

##### Namespace

FreieWahl.Application.Registrations

##### Summary

Handles challenges for users providing proof of id using e.g. SMS

<a name='P-FreieWahl-Application-Registrations-IChallengeService-SupportedChallengeType'></a>
### SupportedChallengeType `property`

##### Summary

the challenge type (e.g. SMS) supported by an implementer

<a name='M-FreieWahl-Application-Registrations-IChallengeService-GetStandardizedRecipient-System-String-'></a>
### GetStandardizedRecipient(recipient) `method`

##### Summary

gets a 'standardized' format for the recipient (e.g. phone number 0664/1234567 => +43664124567) as required by the implementer of this service

##### Returns

the recipient's phone number or address in a standardized format

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| recipient | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the recipient's phone number or address |

<a name='M-FreieWahl-Application-Registrations-IChallengeService-SendChallenge-System-String,System-String,System-String,System-String-'></a>
### SendChallenge(recipient,votingName,challenge,votingId) `method`

##### Summary

sends a challenge to a registered voter

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| recipient | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the recipient's phone number or address |
| votingName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting the voter has registered for |
| challenge | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the challenge that should be sent to the voter |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the voting the voter has registered for |

<a name='T-FreieWahl-Application-Registrations-IRegistrationHandler'></a>
## IRegistrationHandler `type`

##### Namespace

FreieWahl.Application.Registrations

##### Summary

interface for granting or denying a registration

<a name='M-FreieWahl-Application-Registrations-IRegistrationHandler-DenyRegistration-System-String,System-String-'></a>
### DenyRegistration(registrationId,userId) `method`

##### Summary

denies a given registration.

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the registration |
| userId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user granting the registration |

<a name='M-FreieWahl-Application-Registrations-IRegistrationHandler-GrantRegistration-System-String,System-String,System-String,System-TimeSpan,System-String-'></a>
### GrantRegistration(registrationId,userId,votingUrl,utcOffset,timezoneName) `method`

##### Summary

grants a given registration. This also calls GrantRegistration in IRemoteTokenStore

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the registration |
| userId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user granting the registration |
| votingUrl | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the url for the voting |
| utcOffset | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | the utc offset of the voting administrator who granted the registration |
| timezoneName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the name of the timezone of the voting administrator who granted the registration |

<a name='T-FreieWahl-Application-Registrations-IRemoteTokenStore'></a>
## IRemoteTokenStore `type`

##### Namespace

FreieWahl.Application.Registrations

##### Summary

interface for the communication with the tokenstore application, which is an integral part of
project buergerchain, but hosted in a separate app engine instance

<a name='M-FreieWahl-Application-Registrations-IRemoteTokenStore-GetChallenge-System-String-'></a>
### GetChallenge(registrationId) `method`

##### Summary

gets a challenge for a given voting registration. This challenge will be signed when granting a registration, thus proving the authority to grant a registration.

##### Returns

the challenge that needs to be signed later on

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the registration |

<a name='M-FreieWahl-Application-Registrations-IRemoteTokenStore-GrantRegistration-System-String,FreieWahl-Voting-Models-StandardVoting,System-String,System-Collections-Generic-List{System-String},System-String,System-TimeSpan,System-String-'></a>
### GrantRegistration(registrationStoreId,voting,signedChallengeString,signedTokens,votingUrl,utcOffset,timezoneName) `method`

##### Summary

grants a given registration

##### Returns

the response from the token store

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| registrationStoreId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the registration |
| voting | [FreieWahl.Voting.Models.StandardVoting](#T-FreieWahl-Voting-Models-StandardVoting 'FreieWahl.Voting.Models.StandardVoting') | the voting the registration belongs to |
| signedChallengeString | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a signed challenge proving that this request actually originates from this application |
| signedTokens | [System.Collections.Generic.List{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.List 'System.Collections.Generic.List{System.String}') | the list of signed, blinded voting tokens |
| votingUrl | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the url for the voting |
| utcOffset | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | the utc offset of the voting administrator who granted the registration |
| timezoneName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the name of the timezone of the voting administrator who granted the registration |

<a name='M-FreieWahl-Application-Registrations-IRemoteTokenStore-InsertPublicKeys-System-String,System-Collections-Generic-IEnumerable{Org-BouncyCastle-Crypto-Parameters-RsaKeyParameters}-'></a>
### InsertPublicKeys(votingId,publicKeys) `method`

##### Summary

sends the public keys for a given voting to the token store. this can be used by the token store to verify the signature of the signed blinded keys

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id |
| publicKeys | [System.Collections.Generic.IEnumerable{Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters}') | the list of public keys for all questions in the voting |

<a name='T-FreieWahl-Application-Authentication-ISessionCookieProvider'></a>
## ISessionCookieProvider `type`

##### Namespace

FreieWahl.Application.Authentication

##### Summary

Provides session cookies in order to allow a user staying logged in for a longer period

<a name='M-FreieWahl-Application-Authentication-ISessionCookieProvider-CreateSessionCookie-System-String-'></a>
### CreateSessionCookie(idToken) `method`

##### Summary

creates a session cookie for a given session id token

##### Returns

a session cooke for the given id token

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| idToken | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a session id token |

<a name='T-FreieWahl-Application-Tracking-ITracker'></a>
## ITracker `type`

##### Namespace

FreieWahl.Application.Tracking

##### Summary

interface for a simple tracker tracking user activity on the site. This is already obsolete, since we are using Google Analytics directly.

<a name='M-FreieWahl-Application-Tracking-ITracker-Track-System-String,System-String,System-String-'></a>
### Track(path,clientAddress,userAgent) `method`

##### Summary

tracks a user actvity

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| path | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the path visited by the user |
| clientAddress | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user's ip address |
| userAgent | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user's agent (browser) identifier |

<a name='M-FreieWahl-Application-Tracking-ITracker-TrackSpending-System-String,System-String-'></a>
### TrackSpending(votingId,cost) `method`

##### Summary

tracks all the money that is spent for a given voting (e.g. for sending SMS)

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the voting |
| cost | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the amount that was spent |

<a name='T-FreieWahl-Application-VotingResults-IVotingChainBuilder'></a>
## IVotingChainBuilder `type`

##### Namespace

FreieWahl.Application.VotingResults

##### Summary

builds a block chain like data structure for the voting results.
For each question in each voting, one block chain is created.

<a name='M-FreieWahl-Application-VotingResults-IVotingChainBuilder-CheckChain-FreieWahl-Voting-Models-Question,System-Collections-Generic-IReadOnlyList{FreieWahl-Voting-Models-Vote}-'></a>
### CheckChain(q,votes) `method`

##### Summary

checks the block chain for a given question - throws an exception if the block chain is broken

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| q | [FreieWahl.Voting.Models.Question](#T-FreieWahl-Voting-Models-Question 'FreieWahl.Voting.Models.Question') | a question |
| votes | [System.Collections.Generic.IReadOnlyList{FreieWahl.Voting.Models.Vote}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IReadOnlyList 'System.Collections.Generic.IReadOnlyList{FreieWahl.Voting.Models.Vote}') | a list of votes that were cast for this question |

<a name='M-FreieWahl-Application-VotingResults-IVotingChainBuilder-GetGenesisValue-FreieWahl-Voting-Models-Question-'></a>
### GetGenesisValue(q) `method`

##### Summary

Gets the "genesis" value, i.e. the signature for the first block. The signature for the
first block is calculated based on the question a vote belongs to, so a change in the question
can be detected later on.

##### Returns

a digest for the given question (default implementation uses Keccak for the digest)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| q | [FreieWahl.Voting.Models.Question](#T-FreieWahl-Voting-Models-Question 'FreieWahl.Voting.Models.Question') | a question |

<a name='M-FreieWahl-Application-VotingResults-IVotingChainBuilder-GetSignature-FreieWahl-Voting-Models-Vote-'></a>
### GetSignature(v) `method`

##### Summary

the signature for a given vote - this is used for all further blocks after the first one.

##### Returns

a digest/signature for the given question (default implementation uses Keccak for the digest)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| v | [FreieWahl.Voting.Models.Vote](#T-FreieWahl-Voting-Models-Vote 'FreieWahl.Voting.Models.Vote') | a vote |

<a name='T-FreieWahl-Application-VotingResults-IVotingResultManager'></a>
## IVotingResultManager `type`

##### Namespace

FreieWahl.Application.VotingResults

##### Summary

Handles storage and retrieval of voting results

<a name='M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String,System-String[]-'></a>
### GetResults(votingId,tokens) `method`

##### Summary

gets all results for a given voting id for a list of tokens - a voter may use this data to check if all their votes were accounted for properly

##### Returns

a list of votes, all cast in the voting with the given id using the given voting tokens

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the id of the voting |
| tokens | [System.String[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String[] 'System.String[]') | the voting tokens |

<a name='M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String-'></a>
### GetResults(votingId) `method`

##### Summary

gets all votes for a given voting

##### Returns

a list of all votes cast in the voting with the given id

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a voting id |

<a name='M-FreieWahl-Application-VotingResults-IVotingResultManager-GetResults-System-String,System-Int32-'></a>
### GetResults(votingId,questionIndex) `method`

##### Summary

gets all votes for a given voting id and the given question

##### Returns

a list of all votes cast in the voting with the given id and the question with the given index

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a voting id |
| questionIndex | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | a question index |

<a name='M-FreieWahl-Application-VotingResults-IVotingResultManager-StoreVote-System-String,System-Int32,System-Collections-Generic-List{System-String},System-String,System-String-'></a>
### StoreVote(votingId,questionIndex,answers,token,signedToken) `method`

##### Summary

Stores a given vote

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id of the voting this vote was cast in |
| questionIndex | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | the index of the question this vote was cast for |
| answers | [System.Collections.Generic.List{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.List 'System.Collections.Generic.List{System.String}') | a list of answer ids (may also be empty) |
| token | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting token that is used for casting this vote |
| signedToken | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the signed voting token - only if the voting token and the signed token match will the vote be stored |
