import { getRequestedSongs, getUserRequests, searchSongs } from "./service.js";
import { requestSong, userID } from "./domain.js";
import { homeDisplaySongs as displaySongs } from "./displayUpdates.js";

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

// displaySongs(true, "travlin");
displaySongs(false);
