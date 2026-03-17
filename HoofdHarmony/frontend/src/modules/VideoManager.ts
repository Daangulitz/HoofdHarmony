import * as THREE from "three";

export class VideoManager {
  /**
   * Creates a hidden video element and returns a Three.js VideoTexture.
   */
  public createVideoTexture(url: string): {
    texture: THREE.VideoTexture;
    video: HTMLVideoElement;
  } {
    const video = document.createElement("video");
    video.src = url;
    video.loop = true;
    video.muted = false;
    video.crossOrigin = "anonymous";
    video.play();

    // The texture maps the video frames onto 3D geometry
    const texture = new THREE.VideoTexture(video);

    // Set color space for better ghost visuals
    texture.colorSpace = THREE.SRGBColorSpace;

    return { texture, video };
  }
}
