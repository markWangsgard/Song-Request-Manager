import { getCurrentlyPlayingSong, getQueue } from "./service.js";

const loader = document.getElementById("loader");
const currentlyPlayingSectionElement = document.getElementById(
  "currentlyPlayingSection"
);
const upNextSectionElement = document.getElementById("queueSection");

const updateQueue = async () => {
  const currentlyPlaying = await getCurrentlyPlayingSong();
  const queue = await getQueue();

  if (currentlyPlaying.error) {
    loader.remove();

    const errorMessageElement = document.getElementById("errorMessage");
    errorMessageElement.classList.remove("d-none");
    console.error("No Music Playing");
  }

  const currentSong = currentlyPlaying.currentSong;
  const currentlyPlayingElement = document.getElementById("currentlyPlaying");
  currentlyPlayingElement.replaceChildren();
  currentlyPlayingElement.appendChild(createSongElement(currentSong));

  const upNextElement = document.getElementById("queue");
  upNextElement.replaceChildren();

  queue.forEach((song) => {
    upNextElement.appendChild(createSongElement(song));
  });
};

await updateQueue();
loader.remove();

currentlyPlayingSectionElement.classList.remove("d-none");
upNextSectionElement.classList.remove("d-none");

function createSongElement(song) {
  const resultElement = document.createElement("figure");
  resultElement.classList.add("d-flex");
  resultElement.classList.add("justify-content-sm-center");
  resultElement.classList.add("align-items-center");
  resultElement.classList.add("ms-4");
  resultElement.classList.add("p-3");
  resultElement.classList.add("me-4");
    resultElement.classList.add("bg-secondary");
  resultElement.classList.add("rounded");
  //   resultElement.classList.add("w-100");

  const imgElement = document.createElement("img");
  imgElement.style.width = "75px";
  imgElement.style.height = "75px";
  imgElement.src = song.imgURL;
  imgElement.alt = song.trackName;

  const textContainer = document.createElement("figcaption");
  textContainer.classList.add("ms-3", "flex-grow-1");
  textContainer.style = "min-width: 0;";

  const titleElement = document.createElement("h3");
  titleElement.classList.add("text-start");
  titleElement.textContent = song.trackName;

  const artistElement = document.createElement("h4");
  artistElement.classList.add("text-start");
  artistElement.textContent = song.artistName;

  textContainer.appendChild(titleElement);
  textContainer.appendChild(artistElement);
  resultElement.appendChild(imgElement);
  resultElement.appendChild(textContainer);

  return resultElement;
}
