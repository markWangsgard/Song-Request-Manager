import { homeDisplaySongs, adminDisplaySongs} from "./displayUpdates.js";
import { myApiUrl } from "./constants.js";

const signalR = globalThis.signalR;
if (!signalR) {
  console.error("SignalR client not found on global scope. Make sure 'scripts/signalr/signalr.js' is included before module scripts.");
} else {
  console.log("SignalR client loaded.");

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${myApiUrl}/songRequestManager`)
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveSongRequestUpdate", () => {
    console.log("Received song request update");
    homeDisplaySongs(false);
    adminDisplaySongs(false);
  });

  connection
    .start()
    .then(() => console.log("SignalR Connected"))
    .catch((err) => console.error("SignalR start failed:", err));
}
