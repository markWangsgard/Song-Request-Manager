import { getRequestedSongs, searchSongs } from "./service.js";
import {
  addSongToPlaylist,
  requestSong,
  songsAddedToPlaylist,
} from "./domain.js";
import {
  currentPlaylist,
  currentUser,
  loadSettingsFromApi,
} from "./constants.js";


export const homeDisplaySongs = async (searching, query = "") => {
  const searchBarElement = document.getElementById("search");
  const resultsContainer = document.getElementById("results");
  // debugger;
  await loadSettingsFromApi();

  document.getElementById("SectionHeader").textContent = searching
    ? `Search Results for "${query}"`
    : `Requested Songs`;

  const results = searching
    ? await searchSongs(query)
    : await getRequestedSongs();
  resultsContainer.replaceChildren();

  if (results.length === 0) {
    const noResultsElement = document.createElement("h4");
    noResultsElement.classList = "mt-4 ms-3 me-3 mb-3";
    noResultsElement.textContent = searching
      ? "No results found."
      : "No songs have been requested yet. Be the first to request a song!";
    resultsContainer.appendChild(noResultsElement);
  } else {
    results.forEach((result) => {
      if (blacklist.includes(result.id)) {
        return;
      }
      const resultElement = document.createElement("figure");
      resultElement.classList.add("d-flex");
      resultElement.classList.add("justify-content-sm-center");
      resultElement.classList.add("align-items-center");
      resultElement.classList.add("ms-4");
      resultElement.classList.add("p-3");
      resultElement.classList.add("responsive-width");
      resultElement.classList.add("me-4");
      resultElement.classList.add("bg-secondary");
      resultElement.classList.add("rounded");

      const imgElement = document.createElement("img");
      imgElement.style.width = "75px";
      imgElement.style.height = "75px";
      imgElement.src = result.imgURL;
      imgElement.alt = result.trackName;

      const textContainer = document.createElement("figcaption");
      textContainer.classList.add("ms-3");
      textContainer.style = "min-width: 0;";

      const titleElement = document.createElement("h3", "fs-6", "w-auto");
      titleElement.textContent = result.trackName;

      const artistElement = document.createElement("h4");
      artistElement.textContent = result.artistName;

      const countElement = document.createElement("p");
      if (!searching) {
        countElement.textContent = `Requested ${result.requestCount} ${
          result.requestCount === 1 ? "time" : "times"
        }`;
      }

      textContainer.appendChild(titleElement);
      textContainer.appendChild(artistElement);
      resultElement.appendChild(imgElement);
      resultElement.appendChild(textContainer);
      if (!searching) {
        textContainer.appendChild(countElement);
      }

      resultsContainer.appendChild(resultElement);

      resultElement.addEventListener("mouseenter", () => {
        resultElement.style.cursor = "pointer";
        resultElement.classList.remove("bg-secondary");
        resultElement.classList.add("bg-primary");
        resultElement.classList.add("text-black");
      });
      resultElement.addEventListener("mouseleave", () => {
        resultElement.classList.add("bg-secondary");
        resultElement.classList.remove("bg-primary");
        resultElement.classList.remove("text-black");
      });
      resultElement.addEventListener("click", async () => {
        searchBarElement.value = "";

        await requestSong(result.id);

        homeDisplaySongs(false);
      });
    });
  }
};

export const adminDisplaySongs = async (searching, query = "") => {
  const searchBarElement = document.getElementById("search");
  const resultsContainer = document.getElementById("results");

  await loadSettingsFromApi();

  document.getElementById("SectionHeader").textContent = searching
    ? `Search Results for "${query}"`
    : `Requested Songs`;

  const results = searching
    ? await searchSongs(query)
    : await getRequestedSongs();
  resultsContainer.replaceChildren();

  if (results.length === 0) {
    const noResultsElement = document.createElement("h4");
    noResultsElement.classList.add("mt-4");
    noResultsElement.textContent = searching
      ? "No results found."
      : "No songs have been requested yet. Be the first to request a song!";
    resultsContainer.appendChild(noResultsElement);
  } else {
    results.forEach((result) => {
      const resultElement = document.createElement("figure");
      resultElement.classList.add("d-flex");
      resultElement.classList.add("justify-content-sm-center");
      resultElement.classList.add("align-items-center");
      resultElement.classList.add("ms-4");
      resultElement.classList.add("p-3");
      //   resultElement.classList.add("responsive-width");
      resultElement.classList.add("me-4");
      resultElement.classList.add("bg-secondary");
      resultElement.classList.add("rounded");
      resultElement.classList.add("w-100");

      const imgElement = document.createElement("img");
      imgElement.style.width = "75px";
      imgElement.style.height = "75px";
      imgElement.src = result.imgURL;
      imgElement.alt = result.trackName;

      const textContainer = document.createElement("figcaption");
      textContainer.classList.add("ms-3", "flex-grow-1");
      textContainer.style = "min-width: 0;";

      const titleElement = document.createElement("h3");
      titleElement.classList.add("text-start");
      titleElement.textContent = result.trackName;

      const artistElement = document.createElement("h4");
      artistElement.classList.add("text-start");
      artistElement.textContent = result.artistName;

      const countElement = document.createElement("p");
      countElement.classList.add("text-start");
      if (!searching) {
        countElement.textContent = `Requested ${result.requestCount} ${
          result.requestCount === 1 ? "time" : "times"
        }`;
      }

      const addOrCheck = songsAddedToPlaylist.includes(result.id)
        ? "check"
        : "add-icon";
      const addIconElement = document.createElement("img");
      addIconElement.id = `add-or-check-${result.id}`;
      addIconElement.src = `images/${addOrCheck}-primary.svg`;
      addIconElement.alt = "Add Song To Playlist";
      addIconElement.style.width = "50px";

      textContainer.appendChild(titleElement);
      textContainer.appendChild(artistElement);
      resultElement.appendChild(imgElement);
      resultElement.appendChild(textContainer);
      if (currentPlaylist !== null) {
        resultElement.appendChild(addIconElement);
      }
      if (!searching) {
        textContainer.appendChild(countElement);
      }

      resultsContainer.appendChild(resultElement);

      resultElement.addEventListener("mouseenter", () => {
        resultElement.style.cursor = "pointer";
        resultElement.classList.remove("bg-secondary");
        resultElement.classList.add("bg-primary");
        resultElement.classList.add("text-black");
        addIconElement.src = `images/${addOrCheck}-body.svg`;
      });
      resultElement.addEventListener("mouseleave", () => {
        resultElement.classList.add("bg-secondary");
        resultElement.classList.remove("bg-primary");
        resultElement.classList.remove("text-black");
        addIconElement.src = `images/${addOrCheck}-primary.svg`;
      });

      addIconElement.addEventListener("mouseenter", () => {
        resultElement.classList.add("bg-body");
        resultElement.classList.remove("bg-primary");
        resultElement.classList.remove("text-black");
        addIconElement.src = `images/${addOrCheck}-primary.svg`;
      });
      addIconElement.addEventListener("mouseleave", () => {
        resultElement.style.cursor = "pointer";
        resultElement.classList.remove("bg-body");
        resultElement.classList.add("bg-primary");
        resultElement.classList.add("text-black");
        addIconElement.src = `images/${addOrCheck}-body.svg`;
      });

      addIconElement.addEventListener("click", async () => {
        if (
          addOrCheck === "add-icon" &&
          currentPlaylist !== null &&
          currentUser !== null
        ) {
          await addSongToPlaylist(currentPlaylist, result);
          adminDisplaySongs();
        }
      });

      resultElement.addEventListener("click", async (e) => {
        if (e.target.id !== `add-or-check-${result.id}`) {
          searchBarElement.value = "";

          await requestSong(result.id);
          adminDisplaySongs(false);
        }
      });
    });
  }
};
