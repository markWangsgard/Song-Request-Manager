import { getSettings, setSettings } from "./service.js";
import { updateRequestsFromSettings } from "./domain.js";

export const myApiUrl = "http://127.0.0.1:5001";
// export const myApiUrl = `https://soutenu-chan-unscheming.ngrok-free.dev`;

// settings
export let currentUser = null;
export let currentPlaylist = null;
export let numbOfAllowedRequests = 3;
export let allowRepeats = true;
export let autoAdd = false;
export const selectedDays = {
  monday: false,
  tuesday: false,
  wednesday: true,
  thursday: false,
  friday: false,
  saturday: false,
  sunday: false,
};
export let autoAddTime = "22:30";

export const loadSettingsFromApi = async () => {
  const settings = await getSettings();

  currentUser = settings.currentUser;
  currentPlaylist = settings.currentPlaylist;
  numbOfAllowedRequests = settings.numbOfAllowedRequests;
  allowRepeats = settings.allowRepeats;
  autoAdd = settings.allowRepeats;
  selectedDays.monday = settings.selectedDays.monday;
  selectedDays.tuesday = settings.selectedDays.tuesday;
  selectedDays.wednesday = settings.selectedDays.wednesday;
  selectedDays.thursday = settings.selectedDays.thursday;
  selectedDays.friday = settings.selectedDays.friday;
  selectedDays.saturday = settings.selectedDays.saturday;
  selectedDays.sunday = settings.selectedDays.sunday;
  autoAddTime = settings.autoAddTime;
};

export const setUser = (user) => {
  currentUser = user;
  setSettings();
  updateRequestsFromSettings();
};
export const setCurrentPlaylist = (playlist) => {
  currentPlaylist = playlist;
  setSettings();
  updateRequestsFromSettings();
};
export const setNumbOfAllowedRequests = (limit) => {
  numbOfAllowedRequests = limit;
  setSettings();
  updateRequestsFromSettings();
};
export const setAllowRepeats = (allowed) => {
  allowRepeats = allowed;
  setSettings();
  updateRequestsFromSettings();
};
export const setAutoAdd = (auto) => {
  autoAdd = auto;
  setSettings();
};
export const setMonday = (monday) => {
  selectedDays.monday = monday;
  setSettings();
  updateRequestsFromSettings();
};
export const setTuesday = (tuesday) => {
  selectedDays.tuesday = tuesday;
  setSettings();
  updateRequestsFromSettings();
};
export const setWednesday = (wednesday) => {
  selectedDays.wednesday = wednesday;
  setSettings();
  updateRequestsFromSettings();
};
export const setThursday = (thursday) => {
  selectedDays.thursday = thursday;
  setSettings();
  updateRequestsFromSettings();
};
export const setFriday = (friday) => {
  selectedDays.friday = friday;
  setSettings();
  updateRequestsFromSettings();
};
export const setSaturday = (saturday) => {
  selectedDays.saturday = saturday;
  setSettings();
  updateRequestsFromSettings();
};
export const setSunday = (sunday) => {
  selectedDays.sunday = sunday;
  setSettings();
  updateRequestsFromSettings();
};
export const setAutoAddTime = (time) => {
  autoAddTime = time;
  setSettings();
  updateRequestsFromSettings();
};
