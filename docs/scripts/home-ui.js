import { getRequestedSongs, getUserRequests, searchSongs } from "./service.js";
import { requestSong, userID } from "./domain.js";

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

const displaySongs = async (searching, query = "") => {
  document.getElementById("SectionHeader").textContent = searching
    ? `Search Results for "${query}"`
    : `Requested Songs`;

  const results = searching
    ? await searchSongs(query)
    : await getRequestedSongs();
  resultsContainer.replaceChildren();

  results.forEach((result) => {
    const resultElement = document.createElement("figure");
    resultElement.classList.add("d-flex");
    resultElement.classList.add("justify-content-sm-center");
      resultElement.classList.add("ms-4");
      resultElement.classList.add("p-3");
      resultElement.classList.add("responsive-width");
      resultElement.classList.add("me-4");
      resultElement.classList.add("bg-secondary");
      resultElement.classList.add("rounded");

    const imgElement = document.createElement("img");
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
      dispatchEvent(updateRequestedSongs);

      displaySongs(false);
    });

    resultElement.addEventListener("updateRequestedSongs", () => {
      displaySongs(false);
    });
  });
};

// displaySongs(true, "travlin");
displaySongs(false);

console.log(await getUserRequests(userID));
