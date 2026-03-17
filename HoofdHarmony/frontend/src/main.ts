import "./style.css";
import { SceneManager } from "./modules/SceneManager";
import { VideoManager } from "./modules/VideoManager";
import * as THREE from "three";

const app = document.querySelector<HTMLDivElement>("#app")!;

app.innerHTML = `
  <div id="overlay">
    <button id="start-btn">START SPOOKY CHOIR</button>
  </div>
  <canvas id="scene"></canvas>
`;

const startBtn = document.querySelector<HTMLButtonElement>("#start-btn")!;

startBtn.addEventListener("click", () => {
  initExperience();
  document.querySelector("#overlay")?.remove();
});

function initExperience() {
  // 1. Initialize the 3D Stage
  const canvas = document.querySelector("#scene") as HTMLCanvasElement;
  const sceneManager = new SceneManager(canvas);

  // 2. Initialize the Video Logic
  const videoManager = new VideoManager();
  const videoUrl = "http://localhost:3000/media/heads/test.mp4";

  const { texture } = videoManager.createVideoTexture(videoUrl);

  // 3. Create a "Ghost Plane" in the 3D world
  // 16:9 aspect ratio (Width 4, Height 2.25)
  const geometry = new THREE.PlaneGeometry(4, 2.25);
  const material = new THREE.MeshBasicMaterial({
    map: texture,
    transparent: true,
    side: THREE.DoubleSide,
  });

  const ghostMesh = new THREE.Mesh(geometry, material);

  // Add the ghost to our 3D scene
  sceneManager.addMesh(ghostMesh);

  console.log("Ghost added to 3D Scene.");
}
