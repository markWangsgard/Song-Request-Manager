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
// const navPopupContainer = document.getElementById("NavigationPopupContainer");
const adminModal = navPopupContainer;

// Close modal if clicking outside the inner box
navPopupContainer.addEventListener('click', (e) => {
  if (e.target === navPopupContainer) {
    closeModal();
  }
});

// Determine base path
const basePath = window.location.origin === "http://127.0.0.1:5500" ? "/docs/" : "/Song-Request-Manager/";

// Modal buttons
document.getElementById("goHome").onclick = () => {
  window.location.href = basePath + "index.html";
};

document.getElementById("goAdmin").onclick = () => {
  window.location.href = basePath + "admin.html";
};

document.getElementById("goAdminSettings").onclick = () => {
  window.location.href = basePath + "admin-settings.html";
};

// Open / close modal
function openModal() {
  adminModal.classList.remove("d-none");
  document.body.classList.add("no-scroll");
}

function closeModal() {
  adminModal.classList.add("d-none");
  document.body.classList.remove("no-scroll");
}

// Logo element
const logo = document.getElementById("logo");

// Disable default context menu on right-click / long-press
logo.addEventListener("contextmenu", (e) => e.preventDefault());

// Double-click / double-tap logic (works for desktop and mobile)
let clickCount = 0;
const clickTimeout = 400; // ms window
let timer = null;

function handleTap() {
  clickCount++;

  if (clickCount === 2) { // double-click / double-tap
    openModal();
    clickCount = 0;
    clearTimeout(timer);
  } else {
    clearTimeout(timer);
    timer = setTimeout(() => {
      window.location.href = basePath + "index.html";
      clickCount = 0;
    }, clickTimeout);
  }
}

// Desktop click
logo.addEventListener("click", (e) => {
  if (e.shiftKey || e.ctrlKey) {
    openModal();
    clickCount = 0;
    clearTimeout(timer);
    return;
  }
  handleTap();
});

// Mobile touch
logo.addEventListener("touchend", (e) => {
  handleTap();
});

// Escape key closes modal
document.addEventListener("keydown", (e) => {
  if (e.key === "Escape") {
    closeModal();
  }
});

