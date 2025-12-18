import { logout, startKeepAlivePing } from "./service.js";
import {
  currentPlaylist,
  currentUser,
  loadSettingsFromApi,
} from "./constants.js";
import { adminDisplaySongs as displaySongs } from "./displayUpdates.js";

const searchBarElement = document.getElementById("search");
const resultsContainer = document.getElementById("results");
let typingTimer;

searchBarElement.addEventListener("input", async (event) => {
  clearTimeout(typingTimer);

  typingTimer = setTimeout(() => {
    resultsContainer.replaceChildren();
    const query = searchBarElement.value.trim();
    displaySongs(query !== "", query);
  }, 100);
});

const logoElement = document.getElementById("logo");
logoElement.addEventListener("click", () => {
  window.location.href = "./admin-settings.html";
});

await loadSettingsFromApi();
if (!currentUser || currentUser.error) {
  await logout();
}

if (currentUser === null) {
  const loginWarningElement = document.getElementById("loginWarning");
  loginWarningElement.classList.remove("d-none");
} else if (currentPlaylist === null) {
  const playlistWarningElement = document.getElementById("playlistWarning");
  playlistWarningElement.classList.remove("d-none");
}

// displaySongs(true, "travlin");
displaySongs(false);
startKeepAlivePing();