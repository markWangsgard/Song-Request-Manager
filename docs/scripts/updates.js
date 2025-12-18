import { homeDisplaySongs, adminDisplaySongs} from "./displayUpdates.js";

console.log(signalR);

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/songRequestManager")
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
  .catch((err) => console.error(err));
