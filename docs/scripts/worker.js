self.onmessage = async function (e) {
    console.log("Worker received message:", e.data);
  while (true) {
    const now = new Date();
    const day = now.getDay();
    const time = now.getHours() * 100 + now.getMinutes();

    // Days: 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    // Time in HHMM format and military time
    // console.log(`Current day: ${day}, time: ${time}`);
    // Currently set for Wednesday at 6:00 PM to 10:45 PM
    if (day === 3 && time >= 1800 && time <= 2245) {
        await fetch(`${e.data.url}/`);
        console.log("Pinged server to keep alive.");
        await sleep(600000); // Wait for 10 minutes before next ping
    }
  }
};


function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}