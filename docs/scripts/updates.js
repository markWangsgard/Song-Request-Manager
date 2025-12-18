import * as signalR from
  "https://cdn.jsdelivr.net/npm/@microsoft/signalr/dist/browser/signalr.min.js";
import { displaySongs as adminDisplaySongs } from "./admin-ui.js";
import { displaySongs as homeDisplaySongs} from "./home-ui.js";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/songRequestManager")
    .withAutomaticReconnect()
    .build();

connection.on("SongRequested", song => {
    adminDisplaySongs(false);
    homeDisplaySongs(false);
});

connection.start()
    .catch(err => console.error(err));
