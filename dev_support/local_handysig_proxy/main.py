# Copyright 2018 Michael Faschinger <michivo@gmail.com>

# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
# associated documentation files (the "Software"), to deal in the Software without restriction,
# including without limitation the rights to use, copy, modify, merge, publish, distribute,
# sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:

# The above copyright notice and this permission notice shall be included in all copies or
# substantial portions of the Software.

# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
# NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
# DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
# OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

import webapp2
import requests
import logging
import json

json_data=open('config.json').read()
data = json.loads(json_data)
localUrl = data['localUrl']
remoteUrl = data['remoteUrl']
redirectUrl = data['redirectUrl']

logging.info('Hello world, hi there!')
print('hello console!')

class MainPage(webapp2.RequestHandler):
    def get(self):
        logging.info('hello hey ho')
        self.response.headers['Content-Type'] = 'text/plain'
        self.response.write('Hello, World! This is Python! Version 1.3')

class SignatureDataUrl(webapp2.RequestHandler):
   def get(self):
       self.response.headers['Content-Type'] = 'text/html'
       name = self.request.get('name')
       respContent = '<!DOCTYPE html><html><head><script>window.top.location.href = "' + redirectUrl + '?name=' + name + '";</script></head></html>'
       self.response.write(respContent)
   def post(self):
       logging.info('forwarding to ')
       logging.info(remoteUrl)

       payload = (('XMLResponse', self.request.get('XMLResponse')), ('ResponseType', self.request.get('ResponseType')))
       queryParams = {'regUid': self.request.get('regUid')}
       r = requests.post(remoteUrl, data=payload, params=queryParams)
       self.response.headers['Content-Type'] = 'text/html'
       respContent = '<!DOCTYPE html><html><head><script>window.top.location.href = "' + redirectUrl + '?name=' + name + '";</script></head></html>'
       self.response.write(respContent)


app = webapp2.WSGIApplication([
    ('/', MainPage),
    (localUrl, SignatureDataUrl)
], debug=True)
