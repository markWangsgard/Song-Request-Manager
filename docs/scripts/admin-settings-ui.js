import { getMe, login } from "./service.js";

const loginButton = document.getElementById("signIn");
loginButton.addEventListener("click", async (e) => {
    e.preventDefault();
    const user = await login();
    console.log(user);
});

const me = await getMe();
console.log(me.profile);