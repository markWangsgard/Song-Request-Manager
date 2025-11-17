import { getRequestedSongs, searchSongs, requestSong } from "./service.js";

const searchBarElement = document.getElementById("search");
const resultsContainer = document.getElementById("results");
let typingTimer;

searchBarElement.addEventListener("input", async (event) => {
    clearTimeout(typingTimer)

    typingTimer = setTimeout(() => {
  resultsContainer.replaceChildren();

  if (searchBarElement.value.trim() === "") {
    displayRequestedSongs();
  } else {
    displaySearchResults(searchBarElement.value.trim());
  }
}, 250);
});

const displayRequestedSongs = async () => {
  document.getElementById("SectionHeader").textContent = `Requested Songs`;

  const results = await getRequestedSongs();

  resultsContainer.replaceChildren();
  results.forEach((result) => {
    const resultElement = document.createElement("div");
    resultElement.classList.add("d-flex", "justify-content-center", "m-4");

    const imgElement = document.createElement("img");
    imgElement.src = result.imgURL;
    imgElement.alt = result.trackName;
    imgElement.style.objectFit = "contain";

    const textContainer = document.createElement("div");
    textContainer.classList.add("ms-3", "text-container");

    const titleElement = document.createElement("h3", "fs-6");
    titleElement.textContent = result.trackName;

    const artistElement = document.createElement("h4");
    artistElement.textContent = result.artistName;

    const countElement = document.createElement("p");
    countElement.textContent = `Requested ${result.requestCount} times`;

    const updateImgHeight = () => {
      imgElement.style.maxHeight = `${textContainer.offsetHeight}px`;
    };

    updateImgHeight();

    if (window.ResizeObserver) {
      const ro = new ResizeObserver(updateImgHeight);
      ro.observe(textContainer);
    } else {
      window.addEventListener("resize", updateImgHeight);
    }

    textContainer.appendChild(titleElement);
    textContainer.appendChild(artistElement);
    textContainer.appendChild(countElement);
    resultElement.appendChild(imgElement);
    resultElement.appendChild(textContainer);

    resultsContainer.appendChild(resultElement);

    
  });
};

const displaySearchResults = async (query) => {
  document.getElementById(
    "SectionHeader"
  ).textContent = `Search Results for "${query}"`;

  const results = await searchSongs(query);

  results.forEach((result) => {
    console.log(result);
    const resultElement = document.createElement("div");
    resultElement.classList.add("d-flex", "justify-content-center", "m-4");

    const imgElement = document.createElement("img");
    imgElement.src = result.imgURL;
    imgElement.alt = result.trackName;
    imgElement.style.objectFit = "contain";

    const textContainer = document.createElement("div");
    textContainer.classList.add("ms-3", "text-container");

    const titleElement = document.createElement("h3", "fs-6");
    titleElement.textContent = result.trackName;

    const artistElement = document.createElement("h4");
    artistElement.textContent = result.artistName;

    const updateImgHeight = () => {
      imgElement.style.maxHeight = `${textContainer.offsetHeight}px`;
    };

    updateImgHeight();

    if (window.ResizeObserver) {
      const ro = new ResizeObserver(updateImgHeight);
      ro.observe(textContainer);
    } else {
      window.addEventListener("resize", updateImgHeight);
    }

    textContainer.appendChild(titleElement);
    textContainer.appendChild(artistElement);
    resultElement.appendChild(imgElement);
    resultElement.appendChild(textContainer);

    resultsContainer.appendChild(resultElement);

    resultElement.addEventListener("mouseenter", () => {
        resultElement.style.cursor = "pointer";
        resultElement.classList.add("bg-secondary");
    });
    resultElement.addEventListener("mouseleave", () => {
        resultElement.classList.remove("bg-secondary");
    });
    resultElement.addEventListener("click", () => {
        searchBarElement.value = "";
        requestSong(result.id);
        displayRequestedSongs();
    });
  });
};

displayRequestedSongs();
