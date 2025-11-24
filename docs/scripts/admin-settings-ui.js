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
} from "./constants.js";
import { getMe, getSettings, login, setSettings } from "./service.js";

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

const addAllEventListeners = async () => {
  loginButton.addEventListener("click", async (e) => {
    e.preventDefault();
    const user = await login();
    // setUser(await getMe());
  });

  selectPlaylistButton.addEventListener("click", (e) => {
    e.preventDefault();
    // Add select Playlist function
  });

  requestLimitInputElement.addEventListener("input", () => {
    setNumbOfAllowedRequests(requestLimitInputElement.value);
  });

  allowRepeatsSwitchElement.addEventListener("input", () => {
    setAllowRepeats(allowRepeatsSwitchElement.checked);
  });

  autoAddSwitchElement.addEventListener("input", () => {
    setAutoAdd(autoAddSwitchElement.checked);
    updateAutoAdd();
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
  })
};

const updateUser = () => {
  const user = currentUser;
  if (user !== null) {
    userNameElement.classList.remove("visually-hidden")
    userNameElement.textContent = `Signed in as ${user.displayName}`;
    const loginButton = document.getElementById("signIn");
    loginButton.textContent = "Sign Out";
    setUser(user);
  }
  else {
    userNameElement.classList.add("visually-hidden")
  }
};

const updatePlaylist = () => {
  // add update playlist function
};

const updateAutoAdd = () => {
  const autoAddSelectionElement = document.getElementById("autoAddSelection");
  if (autoAdd)
  {
    autoAddSelectionElement.classList.remove("visually-hidden")
  }
  else {
    autoAddSelectionElement.classList.add("visually-hidden")
  }
}

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

addAllEventListeners();
await loadSettingsFromApi();
loadSettings();
 await setSettings();