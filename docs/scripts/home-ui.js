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

// displaySongs(true, "travlin");
displaySongs(false);

// const logo = document.getElementById("admin-logo");

// /* ===== Desktop: Shift + Click ===== */
// logo.addEventListener("click", (e) => {
//   if (e.shiftKey) {
//     if (window.location.origin == "http://127.0.0.1:5500") {
//       window.location.href = "/docs/admin-settings.html";
//     }
//     else {
//       window.location.href = "/admin-settings.html";
//     }
//   }
// });

// /* ===== Mobile: Long Press ===== */
// let pressTimer;

// logo.addEventListener("touchstart", () => {
//   pressTimer = setTimeout(() => {
//     window.location.href = "/admin-settings.html";
//   }, 1200); // 1.2 

