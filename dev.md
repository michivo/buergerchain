Buergerchain is a cloud-based web application hosted in [Google’s app engine](https://cloud.google.com/appengine/). 
If you want to contribute, fork or run your own buergerchain, it is required that you have some knowledge regarding 
Google’s App Engine - it is very well documented, so this documentation will not cover any App Engine-specific information.
The entire buergerchain application is currently split into 2 app engine projects:

## Project *tokenstore*:
Project *tokenstore* is an app engine standard project storing all voter-related data. 
The application logic is implemented in Node.js, the data is stored in Google’s NoSQL-database datastore. 
The *tokenstore* does not provide a user interface, it only serves as an API for the client and server side of project *application*. 
Once a voter registers for a voting, a set of so called tokens is generated that is later used when casting a vote. 
These tokens and all data associated with them is stored and administered by project *tokenstore*. 
The exact process of token generation, signing and validation is described [here](tokens.md).

### Tools & environment
- Node.js/npm
- Visual Studio Code was used to implement the tokenstore, but you are free to choose whatever IDE you like.

## Project *application*:
Project application is an app engine flexible project. It contains the major part of the application logic, 
is implemented in C#/ASP.NET Core. It just hosts a database (Google’s NoSQL firestore) and stores all votings and voting results. 
It is intentionally separated from the *tokenstore* to make sure that data linked to the voter’s identity 
(stored in the *tokenstore*) cannot be linked with the voting results (stored in project *application*).

### Tools & environment
- Visual Studio Community 2017
- ASP.NET Core 2.1

## Further reading
- [Documentation of the blockchain-like datastructure used to store voting results](storage.md)
- [Documentation of the 'Handysignatur Proxy' used in the project](dev_support/README.md) - In order to locally test the 'Handysignatur', it is required to jump through some hoops. This small utility should make it a bit easier to do so.
