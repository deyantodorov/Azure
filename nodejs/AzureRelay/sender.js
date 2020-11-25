const webSocket = require('hyco-ws');
const readline = require('readline')
    .createInterface({
        input: process.stdin,
        output: process.stdout
    });

const ns = "mydtrelay.servicebus.windows.net";
const path = "checkconnection";
const keyrule = "RootManageSharedAccessKey";
const key = "94rHzaDD3DxI1OYxB6O2mKF959pmrb0/01yQAOOIZGg=";

webSocket.relayedConnect(
    webSocket.createRelaySendUri(ns, path),
    webSocket.createRelayToken('http://' + ns, keyrule, key),
    function (wss) {
        readline.on('line', (input) => {
            wss.send(input, null);
        });

        console.log('Started client interval.');
        wss.on('close', function () {
            console.log('stopping client interval');
            process.exit();
        });
    }
);