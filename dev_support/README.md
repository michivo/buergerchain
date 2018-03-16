# Handysignatur App Engine Proxy
If you want to run an application with Handysignatur support locally, you will soon run into the 
following problem: A-Trust will only send you the reply for a Handysignatur request via https, and
only if you are providing a certificate issued by one of the
[authorities accepted by a-trust](https://labs.a-trust.at/developer/ShowArticle.aspx?id=106).
This condition will probably not hold for your local development environment. An alternative that
is viable for debugging purposes is to host a kind of proxy that will receive the responses to 
Handysignatur requests and forwards them to a given URL.

The application __handysig_proxy__ provides exactly this. It implemens a Handysignatur proxy that
can be hosted in a Google App Engine Standard Environment. All you need to do is set the two values
for __localUrl__ and __remoteUrl__ in the config.json properly and deploy the application to Google
App Engine.

__localUrl__ is supposed to contain the relative local app engine URL where your proxy should be
reachable. The Handysignatur proxy will be available at 
https://*app-engine-project-id*.appspot.com/*localUrl*

__remoteUrl__ is supposed to contain the URL that all responses received through the local URL
mentioned above will be routed to. So if you are running your application locally on a machine with
IP address 123.45.67.89, your **remoteUrl** will be something like _http://123.45.67.89:8080/App_ .
