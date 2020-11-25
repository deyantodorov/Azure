const webSocket = require('hyco-ws');
const ns = "mydtrelay.servicebus.windows.net";
const path = "checkconnection";
const keyrule = "RootManageSharedAccessKey";
const key = "94rHzaDD3DxI1OYxB6O2mKF959pmrb0/01yQAOOIZGg=";

var wss = webSocket.createRelayedServer(
    {
        server: webSocket.createRelayListenUri(ns, path),
        token: webSocket.createRelayToken('http://' + ns, keyrule, key)
    },
    function (ws) {
        console.log('connection accepted');
        ws.onmessage = function (event) {
            console.log(event.data);
        };
        ws.on('close', function () {
            console.log('connection closed');
        });
    }
);

console.log('listening');

wss.on('error', function (err) {
    console.log('error' + err);
});