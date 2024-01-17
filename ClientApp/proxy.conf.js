const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:26136';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/home"
   ],
    proxyTimeout: 10000,
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  },
  {
    context: [
      "/SignalR"
    ],
    target: target,
    secure: false,
    ws: true,
  }
]

module.exports = PROXY_CONFIG;
