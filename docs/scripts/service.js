import {
  allowRepeats,
  autoAdd,
  autoAddQuantity,
  autoAddTime,
  currentPlaylist,
  currentUser,
  loadSettingsFromApi,
  myApiUrl,
  numbOfAllowedRequests,
  selectedDays,
  setCurrentPlaylist,
  setUser,
} from "./constants.js";
import { userID } from "./domain.js";

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
  // console.log("login");
  window.location = `${myApiUrl}/login/${userID}?returnTo=${encodeURIComponent(
    window.location.href
  )}`;
  const response = await fetch(`${myApiUrl}/me/${userID}`);
  const user = await response.json();
  return user;
};

export const logout = async () => {
  document.cookie.split(";").forEach((cookie) => {
    const eqPos = cookie.indexOf("=");
    const name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
    document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/";
  });
  // setUser(null);
  // setCurrentPlaylist(null);
  await fetch(`${myApiUrl}/logout/${userID}`);
  await loadSettingsFromApi();
};

export const getMe = async () => {
  const response = await fetch(`${myApiUrl}/me/${userID}`);
  const user = await response.json();
  return user;
};

export const setSettings = async () => {
  if (currentUser && !currentUser.error) {
    const settings = {
      currentPlaylist,
      numbOfAllowedRequests,
      allowRepeats,
      autoAdd,
      autoAddQuantity,
      selectedDays,
      autoAddTime,
    };
    const jsonString = JSON.stringify(settings);
    await fetch(`${myApiUrl}/store-settings/${userID}`, {
      body: JSON.stringify(settings),
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });
  }
};

export const getSettings = async () => {
  const response = await fetch(`${myApiUrl}/get-settings`);
  const settings = await response.json();
  const me = await getMe();
  const updatedSettings = { ...settings, isAdmin: me !== null };
  // console.log(settings);
  setUser(me);
  return updatedSettings;
};

export const getPlaylists = async () => {
  const response = await fetch(`${myApiUrl}/me/${userID}/playlists`);
  const playlists = await response.json();
  return playlists;
};

export const addSongToPlaylistAPI = async (playlistId, songId) => {
  const response = await fetch(
    `${myApiUrl}/playlist/${playlistId}/${userID}/add-song/${songId}`
  );
};

export const clearRequestsAPI = async () => {
  await fetch(`${myApiUrl}/clear-requests`);
};