<a name='assembly'></a>
# FreieWahl.Mail

## Contents

- [IMailProvider](#T-FreieWahl-Mail-IMailProvider 'FreieWahl.Mail.IMailProvider')
  - [SendMail(recipientAddresses,subject,content,args)](#M-FreieWahl-Mail-IMailProvider-SendMail-System-Collections-Generic-List{System-String},System-String,System-String,System-Collections-Generic-Dictionary{System-String,System-String}- 'FreieWahl.Mail.IMailProvider.SendMail(System.Collections.Generic.List{System.String},System.String,System.String,System.Collections.Generic.Dictionary{System.String,System.String})')

<a name='T-FreieWahl-Mail-IMailProvider'></a>
## IMailProvider `type`

##### Namespace

FreieWahl.Mail

##### Summary

Implementations of this interface support sending e-mails to one or more recipients

<a name='M-FreieWahl-Mail-IMailProvider-SendMail-System-Collections-Generic-List{System-String},System-String,System-String,System-Collections-Generic-Dictionary{System-String,System-String}-'></a>
### SendMail(recipientAddresses,subject,content,args) `method`

##### Summary

Sends a mail to one or more recipients. It also supports replacing placeholders in the content with values.

##### Returns

the result of the send process (true or false)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| recipientAddresses | [System.Collections.Generic.List{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.List 'System.Collections.Generic.List{System.String}') | the address(es) of the recipient(s) |
| subject | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the mail subject |
| content | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | the mail content |
| args | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | a list of placeholders and values. All occurences of the placeholder(s) in the content are replaces with the corresponding value(s). |
