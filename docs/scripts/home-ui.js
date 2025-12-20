import {
  getRequestedSongs,
  getUserRequests,
  searchSongs,
  waitForApiAndReload,
} from "./service.js";
import { requestSong, userID } from "./domain.js";
import { homeDisplaySongs as displaySongs } from "./displayUpdates.js";
import { loadSettingsFromApi } from "./constants.js";

const searchBarElement = document.getElementById("search");
const resultsContainer = document.getElementById("results");
let typingTimer;

const updateRequestedSongs = new Event("updateRequestedSongs");

searchBarElement.addEventListener("input", async (event) => {
  clearTimeout(typingTimer);

  typingTimer = setTimeout(() => {
    resultsContainer.replaceChildren();
    const query = searchBarElement.value.trim();
    displaySongs(query !== "", query);
  }, 100);
});

try {
  await loadSettingsFromApi();
} catch (e) {
  const loader = document.getElementById("loader");
  if (loader) loader.classList.remove("d-none");
  waitForApiAndReload();
  throw e;
}

displaySongs(false);

console.log(window.location.origin);