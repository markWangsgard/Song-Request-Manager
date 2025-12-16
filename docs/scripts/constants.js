import { getSettings, setSettings } from "./service.js";

// export const myApiUrl = "http://127.0.0.1:5001";
export const myApiUrl = "https://song-request-manager.onrender.com";

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

  // console.log(settings);
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
  isAdmin = settings.isAdmin;
};

export let isAdmin = false;

export const setUser = (user) => {
  currentUser = user;
  setSettings();
};
export const setCurrentPlaylist = (playlist) => {
  currentPlaylist = playlist;
  setSettings();
};
export const setNumbOfAllowedRequests = (limit) => {
  numbOfAllowedRequests = limit;
  setSettings();
};
export const setAllowRepeats = (allowed) => {
  allowRepeats = allowed;
  setSettings();
};
export const setAutoAdd = (auto) => {
  autoAdd = auto;
  setSettings();
};
export const setMonday = (monday) => {
  selectedDays.monday = monday;
  setSettings();
};
export const setTuesday = (tuesday) => {
  selectedDays.tuesday = tuesday;
  setSettings();
};
export const setWednesday = (wednesday) => {
  selectedDays.wednesday = wednesday;
  setSettings();
};
export const setThursday = (thursday) => {
  selectedDays.thursday = thursday;
  setSettings();
};
export const setFriday = (friday) => {
  selectedDays.friday = friday;
  setSettings();
};
export const setSaturday = (saturday) => {
  selectedDays.saturday = saturday;
  setSettings();
};
export const setSunday = (sunday) => {
  selectedDays.sunday = sunday;
  setSettings();
};
export const setAutoAddTime = (time) => {
  autoAddTime = time;
  setSettings();
};
