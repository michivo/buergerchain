<a name='assembly'></a>
# FreieWahl.UserData

## Contents

- [IUserDataStore](#T-FreieWahl-UserData-Store-IUserDataStore 'FreieWahl.UserData.Store.IUserDataStore')
  - [GetUserImage(userId)](#M-FreieWahl-UserData-Store-IUserDataStore-GetUserImage-System-String- 'FreieWahl.UserData.Store.IUserDataStore.GetUserImage(System.String)')
  - [SaveUserImage(userId,imageData)](#M-FreieWahl-UserData-Store-IUserDataStore-SaveUserImage-System-String,System-String- 'FreieWahl.UserData.Store.IUserDataStore.SaveUserImage(System.String,System.String)')

<a name='T-FreieWahl-UserData-Store-IUserDataStore'></a>
## IUserDataStore `type`

##### Namespace

FreieWahl.UserData.Store

##### Summary

Stores additional user data (main user data like user name, mail, password, ... is handled by firebase auth).
Currently, this additional user data only consists of the user image.

<a name='M-FreieWahl-UserData-Store-IUserDataStore-GetUserImage-System-String-'></a>
### GetUserImage(userId) `method`

##### Summary

gets the image for a user

##### Returns

the string representation of the user image

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| userId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user's id |

<a name='M-FreieWahl-UserData-Store-IUserDataStore-SaveUserImage-System-String,System-String-'></a>
### SaveUserImage(userId,imageData) `method`

##### Summary

Saves the image for a user

##### Returns

the future of this operation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| userId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the user's id |
| imageData | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | a string representation of the user image |
