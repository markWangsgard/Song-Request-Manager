import { myApiUrl } from "./constants.js";

// const myApiUrl = "https://soutenu-chan-unscheming.ngrok-free.dev/";

export const searchSongs = async (songTitle) => {
  const response = await fetch(`${myApiUrl}/search/${songTitle}`);
  const songs = await response.json();
  return songs;
};

export const getRequestedSongs = async () => {
    const response = await fetch(`${myApiUrl}/requested-songs`);
    const songs = await response.json();
    return songs;
    
};

export const requestSong = async (songID) => {
  await fetch(`${myApiUrl}/request-song/${songID}`);
};
