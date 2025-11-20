import { allowRepeats, numbOfAllowedRequests } from "./constants.js";
import { getUserRequests, RemoveSong, requestSongAPI } from "./service.js";
let userID = window.localStorage.getItem("userID");

if (!userID) {
  userID = crypto.randomUUID();
  window.localStorage.setItem("userID", userID);
}

const limitReached = async () => {
  const requests = await getUserRequests(userID);
  if (requests.statusCode) {
    return false;
  }
  return requests.length >= numbOfAllowedRequests;
};

export const requestSong = async (songID) => {
  const usersSongs = await getUserRequests(userID);
  if (await limitReached()) {
    RemoveSong(userID, usersSongs[0].id);
  }
  if (allowRepeats || usersSongs.statusCode) {
    await requestSongAPI(userID, songID);
  } else {
    if (usersSongs.filter(s => s.id === songID).length === 0) {
      await requestSongAPI(userID, songID);
    }
  }
};
