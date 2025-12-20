// Create the main container div
const navPopupContainer = document.createElement('div');
navPopupContainer.id = 'NavigationPopupContainer';
navPopupContainer.className = 'd-none bg-black vw-100 vh-100 fixed-top d-flex justify-content-center align-items-start text-center';
navPopupContainer.style.setProperty('--bs-bg-opacity', '0.75');

// Create the inner box div
const innerBox = document.createElement('div');
innerBox.className = 'bg-body mt-5 w-75 h-25 rounded-5 d-flex flex-column justify-content-evenly';

// Create the heading
const heading = document.createElement('h1');
heading.className = 'mt-4 p-2';
heading.textContent = 'What page would you like to go to?';

// Create the buttons container
const buttonsContainer = document.createElement('div');
buttonsContainer.className = 'mb-4 d-flex justify-content-evenly';

// Create buttons
const buttons = [
  { id: 'goHome', text: 'Home' },
  { id: 'goAdmin', text: 'Admin' },
  { id: 'goAdminSettings', text: 'Admin Settings' }
];

buttons.forEach(btnInfo => {
  const button = document.createElement('button');
  button.id = btnInfo.id;
  button.className = 'btn btn-outline-primary';
  button.textContent = btnInfo.text;
  buttonsContainer.appendChild(button);
});

// Assemble the elements
innerBox.appendChild(heading);
innerBox.appendChild(buttonsContainer);
navPopupContainer.appendChild(innerBox);

// Insert at the very start of the body
document.body.insertBefore(navPopupContainer, document.body.firstChild);




// Navigation Modal Logic

const adminModal = document.getElementById("NavigationPopupContainer");

// Determine base path
const basePath = window.location.origin === "https://markwangsgard.github.io/Song-Request-Manager/" ? "/Song-Request-Manager/" : "/docs/";

// Buttons
document.getElementById("goHome").onclick = () => {
  window.location.href = basePath + "index.html";
};

document.getElementById("goAdmin").onclick = () => {
  window.location.href = basePath + "admin.html";
};

document.getElementById("goAdminSettings").onclick = () => {
  window.location.href = basePath + "admin-settings.html";
};

function openModal() {
  adminModal.classList.remove("d-none");
}

// Logo element
const logo = document.getElementById("logo");

// Long-press / Shift+Click / Ctrl+Click
let pressTimer;
const longPressDuration = 600; // ms

logo.addEventListener("mousedown", startPress);
logo.addEventListener("touchstart", startPress);

logo.addEventListener("mouseup", cancelPress);
logo.addEventListener("mouseleave", cancelPress);
logo.addEventListener("touchend", cancelPress);

function startPress(e) {
  // Desktop: Shift+Click or Ctrl+Click opens modal
  if (e.shiftKey || e.ctrlKey) {
    console.log("Opening admin modal");
    openModal();
    return;
  }

  // Mobile: start long-press timer
  pressTimer = setTimeout(() => {
    openModal(); // long-press on mobile
  }, longPressDuration);
}

function cancelPress() {
  clearTimeout(pressTimer);
}

// Normal click goes to Home
logo.addEventListener("click", (e) => {
  if (!e.shiftKey && !e.ctrlKey) {
    window.location.href = basePath + "index.html";
  }
});
