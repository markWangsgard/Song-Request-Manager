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

export const getUserRequests = async (userId) => {
  const response = await fetch(`${myApiUrl}/get-user-requests/${userId}`);
  const userRequests = await response.json();
  return userRequests;
};

export const requestSongAPI = async (userID, songID) => {
  await fetch(`${myApiUrl}/request-song/${userID}/${songID}`);
  const songs = await getRequestedSongs();
};

export const RemoveSong = async (userID, songID) => {
    await fetch(`${myApiUrl}/remove-song/${userID}/${songID}`);
};

export const login = async () => {
  window.location = `${myApiUrl}/login?returnTo=${encodeURIComponent(window.location.href)}`
  const response = await fetch(`${myApiUrl}/me`);
  const user = await response.json();
  return user;
};

export const getMe = async () => {
  const response = await fetch(`${myApiUrl}/me`);
  const user = await response.json();
  return user;
}