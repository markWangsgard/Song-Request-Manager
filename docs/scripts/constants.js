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
    sunday: false
};
export let autoAddTime = "22:30"

export const setUser = (user) => {
    currentUser = user
};
export const setCurrentPlaylist = (playlist) => {
    currentPlaylist = playlist
}
export const setNumbOfAllowedRequests = (limit) => {
    numbOfAllowedRequests = limit;
}
export const setAllowRepeats = (allowed) => {
    allowRepeats = allowed;
}
export const setAutoAdd = (auto) => {
    autoAdd = auto;
}
export const setMonday = (monday) => {
    selectedDays.monday = monday;
}
export const setTuesday = (tuesday) => {
    selectedDays.tuesday = tuesday;
}
export const setWednesday = (wednesday) => {
    selectedDays.wednesday = wednesday;
}
export const setThursday = (thursday) => {
    selectedDays.thursday = thursday;
}
export const setFriday = (friday) => {
    selectedDays.friday = friday;
}
export const setSaturday = (saturday) => {
    selectedDays.saturday = saturday;
}
export const setSunday = (sunday) => {
    selectedDays.sunday = sunday;
}
export const setAutoAddTime = (time) => {
    autoAddTime = time;
}