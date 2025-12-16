import {
  allowRepeats,
  autoAdd,
  autoAddTime,
  numbOfAllowedRequests,
  selectedDays,
  setAllowRepeats,
  setAutoAdd,
  setMonday,
  setTuesday,
  setWednesday,
  setThursday,
  setFriday,
  setSaturday,
  setSunday,
  setNumbOfAllowedRequests,
  setUser,
  setAutoAddTime,
  loadSettingsFromApi,
  currentUser,
  currentPlaylist,
  setCurrentPlaylist,
  isAdmin,
} from "./constants.js";
import { songsAddedToPlaylist, userID } from "./domain.js";
import { getPlaylists, login, setSettings, logout } from "./service.js";

const bodyElement = document.getElementById("body");
const loginButton = document.getElementById("signIn");
const selectPlaylistButton = document.getElementById("selectPlaylist");
const userNameElement = document.getElementById("userName");
const requestLimitInputElement = document.getElementById("requestLimit");
const autoAddSwitchElement = document.getElementById("autoAdd");
const allowRepeatsSwitchElement = document.getElementById("allowRepeats");
const tuesdayCheckboxElement = document.getElementById("dayTuesday");
const mondayCheckboxElement = document.getElementById("dayMonday");
const thursdayCheckboxElement = document.getElementById("dayThursday");
const wednesdayCheckboxElement = document.getElementById("dayWednesday");
const saturdayCheckboxElement = document.getElementById("daySaturday");
const fridayCheckboxElement = document.getElementById("dayFriday");
const timePickerElement = document.getElementById("timePicker");
const sundayCheckboxElement = document.getElementById("daySunday");
const cancelChoicePlaylistButton = document.getElementById(
  "CancelPlaylistSelect"
);
const selectChoicePlaylistButton = document.getElementById(
  "SelectPlaylistSelect"
);
let tempSelectedPlaylist = currentPlaylist;

const addAllEventListeners = async () => {
  const logoElement = document.getElementById("logo");
  logoElement.addEventListener("click", () => {
    window.location.href = "./admin.html";
  });

  loginButton.addEventListener("click", async (e) => {
    e.preventDefault();
    console.log(currentUser);
    if (currentUser) {
      await logout();
    } else {
      await login();
    }
    updateUser();
  });

  selectPlaylistButton.addEventListener("click", async (e) => {
    e.preventDefault();
    await displayPlaylists();
    const popupElement = document.getElementById("selectPlaylistContainer");
    popupElement.classList.remove("d-none");
    bodyElement.classList.add("no-scroll");
    // bodyElement.style.scrollBehavior = "hidden";
  });

  requestLimitInputElement.addEventListener("input", () => {
    setNumbOfAllowedRequests(requestLimitInputElement.value);
  });

  allowRepeatsSwitchElement.addEventListener("input", () => {
    setAllowRepeats(allowRepeatsSwitchElement.checked);
  });

  autoAddSwitchElement.addEventListener("input", () => {
    setAutoAdd(autoAddSwitchElement.checked);
    const autoAddSelectionElement = document.getElementById("autoAddSelection");
    if (autoAdd) {
      autoAddSelectionElement.classList.remove("visually-hidden");
    } else {
      autoAddSelectionElement.classList.add("visually-hidden");
    }
  });

  mondayCheckboxElement.addEventListener("input", () => {
    setMonday(mondayCheckboxElement.checked);
  });

  tuesdayCheckboxElement.addEventListener("input", () => {
    setTuesday(tuesdayCheckboxElement.checked);
  });

  wednesdayCheckboxElement.addEventListener("input", () => {
    setWednesday(wednesdayCheckboxElement.checked);
  });

  thursdayCheckboxElement.addEventListener("input", () => {
    setThursday(thursdayCheckboxElement.checked);
  });

  fridayCheckboxElement.addEventListener("input", () => {
    setFriday(fridayCheckboxElement.checked);
  });

  saturdayCheckboxElement.addEventListener("input", () => {
    setSaturday(saturdayCheckboxElement.checked);
  });

  sundayCheckboxElement.addEventListener("input", () => {
    setSunday(sundayCheckboxElement.checked);
  });

  timePickerElement.addEventListener("input", () => {
    setAutoAddTime(timePickerElement.value);
  });

  cancelChoicePlaylistButton.addEventListener("click", () => {
    const popupElement = document.getElementById("selectPlaylistContainer");
    popupElement.classList.add("d-none");
    bodyElement.classList.remove("no-scroll");
    updatePlaylist();
  });

  selectChoicePlaylistButton.addEventListener("click", () => {
    const popupElement = document.getElementById("selectPlaylistContainer");
    popupElement.classList.add("d-none");
    bodyElement.classList.remove("no-scroll");
    setCurrentPlaylist(tempSelectedPlaylist);
    updatePlaylist();
  });
};

const updateUser = () => {
  const user = currentUser;
  const allSettingsElement = document.getElementById("allSettings");
  if (user) {
    userNameElement.classList.remove("visually-hidden");
    userNameElement.textContent = `Signed in as ${user.displayName}`;
    const loginButton = document.getElementById("signIn");
    loginButton.textContent = "Sign Out";
    if (isAdmin)
    {
      allSettingsElement.classList.remove("d-none");
    }
    
    setUser(user);
  } else {
    const loginButton = document.getElementById("signIn");
    loginButton.textContent = "Sign In";
    userNameElement.classList.add("visually-hidden");
    allSettingsElement.classList.add("d-none");
  }
};

const updatePlaylist = () => {
  const selectedPlaylistTextElement = document.getElementById(
    "playlistSelectedText"
  );
  if (currentPlaylist !== null) {
    selectedPlaylistTextElement.textContent = `Playlist selected: ${currentPlaylist.Name}`;
    selectedPlaylistTextElement.classList.remove("visually-hidden");
  } else {
    selectedPlaylistTextElement.classList.add("visually-hidden");
  }
  songsAddedToPlaylist.length = 0;
};

const updateAutoAdd = () => {};

const loadSettings = () => {
  updateUser();
  updatePlaylist();
  updateAutoAdd();

  requestLimitInputElement.value = numbOfAllowedRequests;

  allowRepeatsSwitchElement.checked = allowRepeats;

  autoAddSwitchElement.checked = autoAdd;

  mondayCheckboxElement.checked = selectedDays.monday;
  tuesdayCheckboxElement.checked = selectedDays.tuesday;
  wednesdayCheckboxElement.checked = selectedDays.wednesday;
  thursdayCheckboxElement.checked = selectedDays.thursday;
  fridayCheckboxElement.checked = selectedDays.friday;
  saturdayCheckboxElement.checked = selectedDays.saturday;
  sundayCheckboxElement.checked = selectedDays.sunday;

  timePickerElement.value = autoAddTime;
};

const displayPlaylists = async () => {
  if (currentUser !== null) {
    const playlists = await getPlaylists();

    const playlistListElement = document.getElementById("playlistList");
    playlistListElement.replaceChildren();
    
    // <div id="playlistExampleElement" class="rounded-5 p-3 m-3 d-flex justify-content-center align-items-center">
    //           <img src="https://i.scdn.co/image/ab67616d00004851f00a1acf866539632b187ea0" alt="Country Music">
    //           <h4 class="ms-4">Country Music</h4>
    //         </div>
    
    playlists.forEach((p) => {
      // console.log(p);
      const playlistElement = document.createElement("div");
      if (tempSelectedPlaylist !== null && p.Id === tempSelectedPlaylist.Id) {
        playlistElement.classList.add("bg-secondary");
      }
      console.log(tempSelectedPlaylist);
      playlistElement.classList.add("rounded-5");
      playlistElement.classList.add("p-3");
      playlistElement.classList.add("m-3");
      playlistElement.classList.add("d-flex");
      playlistElement.classList.add("justify-content-center");
      playlistElement.classList.add("align-items-center");
      playlistListElement.appendChild(playlistElement);

      const playlistImageElement = document.createElement("img");
      playlistImageElement.style = "height: 75px;";
      playlistImageElement.src = p.ImgUrl ?? "./images/missing-image.svg";
      playlistImageElement.alt = p.Name;
      playlistElement.appendChild(playlistImageElement);

      const playlistTextElement = document.createElement("h4");
      playlistTextElement.classList.add("ms-4");
      playlistTextElement.textContent = p.Name;
      playlistElement.appendChild(playlistTextElement);

      playlistElement.addEventListener("mouseenter", () => {
        playlistElement.style.cursor = "pointer";
        playlistElement.classList.remove("bg-body");
        playlistElement.classList.remove("bg-secondary");
        playlistElement.classList.add("bg-primary");
        playlistElement.classList.add("text-black");
      });
      playlistElement.addEventListener("mouseleave", () => {
        if (tempSelectedPlaylist !== null && p.Id === tempSelectedPlaylist.Id) {
          playlistElement.classList.add("bg-secondary");
        } else {
          playlistElement.classList.add("bg-body");
        }
        playlistElement.classList.remove("bg-primary");
        playlistElement.classList.remove("text-black");
      });

      playlistElement.addEventListener("click", () => {
        if (tempSelectedPlaylist !== null && p.Id === tempSelectedPlaylist.Id) {
          tempSelectedPlaylist = null;
          displayPlaylists();
          return;
        }

        tempSelectedPlaylist = p;
        displayPlaylists();
      });
    });
  }
};


addAllEventListeners();
await loadSettingsFromApi();
console.log(currentUser);
loadSettings();
const loaderElement = document.getElementById("loader");
loaderElement.classList.add("d-none");
const loginSectionElement = document.getElementById("loginSection");
loginSectionElement.classList.remove("d-none");
// if (currentUser === null) {
//   await login();
// }
await setSettings();