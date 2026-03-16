import "./style.css";

const app = document.querySelector<HTMLDivElement>("#app")!;

// 1. Create a Start Overlay (Required for Audio/Video autoplay policy)
app.innerHTML = `
  <div id="overlay">
    <button id="start-btn">START SPOOKY CHOIR</button>
  </div>
  <canvas id="scene"></canvas>
`;

const startBtn = document.querySelector<HTMLButtonElement>("#start-btn")!;

startBtn.addEventListener("click", () => {
  initExperience();
  // Remove the overlay once clicked
  document.querySelector("#overlay")?.remove();
});

function initExperience() {
  console.log("Initializing Systems...");

  const videoUrl = "http://localhost:3000/media/heads/test.mp4";

  const video = document.createElement("video");
  video.src = videoUrl;
  video.loop = true;
  video.muted = false; // Now we can unmute because the user clicked!

  // Add to body hidden, or just keep it in memory for Three.js
  //video.style.display = "none";
  video.style.display = "block";
  video.style.position = "absolute";
  video.style.top = "0";
  video.style.zIndex = "10";
  document.body.appendChild(video);

  video.play().catch((e) => console.error("Video failed:", e));

  // TODO: Initialize your SceneManager and AudioManager here
}
