<a name='assembly'></a>
# FreieWahl.Security

## Contents

- [IJwtAuthentication](#T-FreieWahl-Security-Authentication-IJwtAuthentication 'FreieWahl.Security.Authentication.IJwtAuthentication')
  - [CheckToken(token)](#M-FreieWahl-Security-Authentication-IJwtAuthentication-CheckToken-System-String- 'FreieWahl.Security.Authentication.IJwtAuthentication.CheckToken(System.String)')
  - [Initialize(certUrl,issuer,audience)](#M-FreieWahl-Security-Authentication-IJwtAuthentication-Initialize-System-String,System-String,System-String- 'FreieWahl.Security.Authentication.IJwtAuthentication.Initialize(System.String,System.String,System.String)')
- [ISignatureHandler](#T-FreieWahl-Security-Signing-Buergerkarte-ISignatureHandler 'FreieWahl.Security.Signing.Buergerkarte.ISignatureHandler')
  - [GetSignedContent(signedData)](#M-FreieWahl-Security-Signing-Buergerkarte-ISignatureHandler-GetSignedContent-System-String- 'FreieWahl.Security.Signing.Buergerkarte.ISignatureHandler.GetSignedContent(System.String)')
- [ISignatureProvider](#T-FreieWahl-Security-Signing-Common-ISignatureProvider 'FreieWahl.Security.Signing.Common.ISignatureProvider')
  - [IsSignatureValid(data,signature)](#M-FreieWahl-Security-Signing-Common-ISignatureProvider-IsSignatureValid-System-Byte[],System-Byte[]- 'FreieWahl.Security.Signing.Common.ISignatureProvider.IsSignatureValid(System.Byte[],System.Byte[])')
  - [SignData(data)](#M-FreieWahl-Security-Signing-Common-ISignatureProvider-SignData-System-Byte[]- 'FreieWahl.Security.Signing.Common.ISignatureProvider.SignData(System.Byte[])')
- [ITimestampService](#T-FreieWahl-Security-TimeStamps-ITimestampService 'FreieWahl.Security.TimeStamps.ITimestampService')
  - [CheckTokenContent(token,data)](#M-FreieWahl-Security-TimeStamps-ITimestampService-CheckTokenContent-Org-BouncyCastle-Tsp-TimeStampToken,System-Byte[]- 'FreieWahl.Security.TimeStamps.ITimestampService.CheckTokenContent(Org.BouncyCastle.Tsp.TimeStampToken,System.Byte[])')
  - [GetToken(data,checkCertificate)](#M-FreieWahl-Security-TimeStamps-ITimestampService-GetToken-System-Byte[],System-Boolean- 'FreieWahl.Security.TimeStamps.ITimestampService.GetToken(System.Byte[],System.Boolean)')
- [IUserHandler](#T-FreieWahl-Security-UserHandling-IUserHandler 'FreieWahl.Security.UserHandling.IUserHandler')
  - [MapUser(result)](#M-FreieWahl-Security-UserHandling-IUserHandler-MapUser-System-Security-Claims-ClaimsPrincipal- 'FreieWahl.Security.UserHandling.IUserHandler.MapUser(System.Security.Claims.ClaimsPrincipal)')
- [IVotingKeyStore](#T-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore 'FreieWahl.Security.Signing.VotingTokens.IVotingKeyStore')
  - [GetKeyPair(votingId,index)](#M-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore-GetKeyPair-System-String,System-Int32- 'FreieWahl.Security.Signing.VotingTokens.IVotingKeyStore.GetKeyPair(System.String,System.Int32)')
  - [StoreKeyPairs(votingId,keys)](#M-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore-StoreKeyPairs-System-String,System-Collections-Generic-Dictionary{System-Int32,Org-BouncyCastle-Crypto-AsymmetricCipherKeyPair}- 'FreieWahl.Security.Signing.VotingTokens.IVotingKeyStore.StoreKeyPairs(System.String,System.Collections.Generic.Dictionary{System.Int32,Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair})')
- [PkiStatus](#T-FreieWahl-Security-TimeStamps-PkiStatus 'FreieWahl.Security.TimeStamps.PkiStatus')
  - [Granted](#F-FreieWahl-Security-TimeStamps-PkiStatus-Granted 'FreieWahl.Security.TimeStamps.PkiStatus.Granted')
  - [GrantedWithMods](#F-FreieWahl-Security-TimeStamps-PkiStatus-GrantedWithMods 'FreieWahl.Security.TimeStamps.PkiStatus.GrantedWithMods')
  - [Rejection](#F-FreieWahl-Security-TimeStamps-PkiStatus-Rejection 'FreieWahl.Security.TimeStamps.PkiStatus.Rejection')
  - [RevocationNotification](#F-FreieWahl-Security-TimeStamps-PkiStatus-RevocationNotification 'FreieWahl.Security.TimeStamps.PkiStatus.RevocationNotification')
  - [RevocationWarning](#F-FreieWahl-Security-TimeStamps-PkiStatus-RevocationWarning 'FreieWahl.Security.TimeStamps.PkiStatus.RevocationWarning')
  - [Waiting](#F-FreieWahl-Security-TimeStamps-PkiStatus-Waiting 'FreieWahl.Security.TimeStamps.PkiStatus.Waiting')

<a name='T-FreieWahl-Security-Authentication-IJwtAuthentication'></a>
## IJwtAuthentication `type`

##### Namespace

FreieWahl.Security.Authentication

##### Summary

Interface for checking JWT authorization tokens, this is essential for the authentication process.

<a name='M-FreieWahl-Security-Authentication-IJwtAuthentication-CheckToken-System-String-'></a>
### CheckToken(token) `method`

##### Summary

checks if a given jwt token is valid (see prerequisites above)

##### Returns

the validation result

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| token | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a token |

<a name='M-FreieWahl-Security-Authentication-IJwtAuthentication-Initialize-System-String,System-String,System-String-'></a>
### Initialize(certUrl,issuer,audience) `method`

##### Summary

Initializes an instance of the token checker

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| certUrl | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | url of the public keys used to sign the JWT tokens (e.g. https://www.googleapis.com/identitytoolkit/v3/relyingparty/publicKeys) - only tokens signed with one of the corresponding private keys are accepted |
| issuer | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | issuer of the jwt token (e.g. https://session.firebase.google.com/freiewahl-application ) - only tokens with this issuer are accepted |
| audience | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | valid audience for jwt tokens (e.g. freiewahl-application) - only tokens published for this audience will be accepted |

<a name='T-FreieWahl-Security-Signing-Buergerkarte-ISignatureHandler'></a>
## ISignatureHandler `type`

##### Namespace

FreieWahl.Security.Signing.Buergerkarte

##### Summary

gets the data, signee and certificate that was used to create the given signed data

<a name='M-FreieWahl-Security-Signing-Buergerkarte-ISignatureHandler-GetSignedContent-System-String-'></a>
### GetSignedContent(signedData) `method`

##### Summary

gets the data, signee and certificate that was used to create the given signed data

##### Returns

the decoded data from the CMS message, i.e. the signee id, signee name, signature certificate and the original data

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| signedData | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | CMS signed data |

<a name='T-FreieWahl-Security-Signing-Common-ISignatureProvider'></a>
## ISignatureProvider `type`

##### Namespace

FreieWahl.Security.Signing.Common

##### Summary

signature provider, supports signing data - in the default implementation, SHA256withRSA is used for signing

<a name='M-FreieWahl-Security-Signing-Common-ISignatureProvider-IsSignatureValid-System-Byte[],System-Byte[]-'></a>
### IsSignatureValid(data,signature) `method`

##### Summary

only used in test code - checks if a signature is valid

##### Returns

true, if the signature is a valid signature for this data

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | some data |
| signature | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | the signature for this data |

<a name='M-FreieWahl-Security-Signing-Common-ISignatureProvider-SignData-System-Byte[]-'></a>
### SignData(data) `method`

##### Summary

signs some data (typically using SHA256withRSA or something similar)

##### Returns

signature for this data

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | some data |

<a name='T-FreieWahl-Security-TimeStamps-ITimestampService'></a>
## ITimestampService `type`

##### Namespace

FreieWahl.Security.TimeStamps

##### Summary

Service for processing time stamp requests according to RFC 3161.

<a name='M-FreieWahl-Security-TimeStamps-ITimestampService-CheckTokenContent-Org-BouncyCastle-Tsp-TimeStampToken,System-Byte[]-'></a>
### CheckTokenContent(token,data) `method`

##### Summary

only used for testing purposes - checks if a timestamp is valid

##### Returns

checks if the timestamp is actually valid

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| token | [Org.BouncyCastle.Tsp.TimeStampToken](#T-Org-BouncyCastle-Tsp-TimeStampToken 'Org.BouncyCastle.Tsp.TimeStampToken') | a timestamp token for some data |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | some data |

<a name='M-FreieWahl-Security-TimeStamps-ITimestampService-GetToken-System-Byte[],System-Boolean-'></a>
### GetToken(data,checkCertificate) `method`

##### Summary

A hash is calculated for the given data, a time stamp token with a timestamp
from a trusted time stamp authority is returned.

##### Returns

a time stamp token issued by a time stamp authority

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | the data for which a time stamp should be created |
| checkCertificate | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | flag whether the certficate used to create the time stamp should be verified, too |

<a name='T-FreieWahl-Security-UserHandling-IUserHandler'></a>
## IUserHandler `type`

##### Namespace

FreieWahl.Security.UserHandling

##### Summary

Maps user information from a JWT auth token to user information including user name, id and mail address

<a name='M-FreieWahl-Security-UserHandling-IUserHandler-MapUser-System-Security-Claims-ClaimsPrincipal-'></a>
### MapUser(result) `method`

##### Summary

Maps user information from a JWT auth token to user information including user name, id and mail address

##### Returns

the extracted user information (user name, id and mail address)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| result | [System.Security.Claims.ClaimsPrincipal](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Security.Claims.ClaimsPrincipal 'System.Security.Claims.ClaimsPrincipal') | user claims from a JWT auth token |

<a name='T-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore'></a>
## IVotingKeyStore `type`

##### Namespace

FreieWahl.Security.Signing.VotingTokens

##### Summary

The voting key store stores the key pairs required for signing blinded voting tokens

<a name='M-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore-GetKeyPair-System-String,System-Int32-'></a>
### GetKeyPair(votingId,index) `method`

##### Summary

gets the key pair for the given voting id and question index

##### Returns

the key pair for the given voting id and question index

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a voting id |
| index | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | the question index |

<a name='M-FreieWahl-Security-Signing-VotingTokens-IVotingKeyStore-StoreKeyPairs-System-String,System-Collections-Generic-Dictionary{System-Int32,Org-BouncyCastle-Crypto-AsymmetricCipherKeyPair}-'></a>
### StoreKeyPairs(votingId,keys) `method`

##### Summary

Stores a list of private/public key pairs

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| votingId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the voting id the list of key pairs belongs to |
| keys | [System.Collections.Generic.Dictionary{System.Int32,Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.Int32,Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair}') | a list of keys - the key is the question index for a key pair, the value is the key pair that is stored |

<a name='T-FreieWahl-Security-TimeStamps-PkiStatus'></a>
## PkiStatus `type`

##### Namespace

FreieWahl.Security.TimeStamps

##### Summary

PKI statuses according to RFC 3161

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-Granted'></a>
### Granted `constants`

##### Summary

When the PKIStatus contains the value zero a TimeStampToken, as requested, is present.

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-GrantedWithMods'></a>
### GrantedWithMods `constants`

##### Summary

When the PKIStatus contains the value one a TimeStampToken, with modifications, is present.

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-Rejection'></a>
### Rejection `constants`

##### Summary

When the PKIStatus contains the value two a TimeStamp request was rejected.

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-RevocationNotification'></a>
### RevocationNotification `constants`

##### Summary

Revocation has occurred.

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-RevocationWarning'></a>
### RevocationWarning `constants`

##### Summary

A warning that a revocation is imminent.

<a name='F-FreieWahl-Security-TimeStamps-PkiStatus-Waiting'></a>
### Waiting `constants`

##### Summary

The request body part has not yet been processed, expect to hear more later.
