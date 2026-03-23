using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SayWhenNoInput : MonoBehaviour
{
    [Header("Head 1 (Jens)")]
    public Image head1Display;
    public Sprite head1Normal, head1Talking;
    public int bruceChannel = 0;

    [Header("Head 2 (Marijn)")]
    public Image head2Display;
    public Sprite head2Normal, head2Talking;
    public int doryChannel = 1;

    [Header("Head 3 (Daan)")]
    public Image head3Display;
    public Sprite head3Normal, head3Talking;
    public int marlinChannel = 2;

    [Header("Audio Sources")]
    public AudioSource performanceSource;
    public AudioSource idleSource;
    public AudioClip itsAWitch;
    public AudioClip[] idleClips;

    [Header("Settings")]
    public bool debugMode = true;
    [Range(0.001f, 0.2f)] public float sensitivity = 0.01f;
    [Range(0.01f, 0.5f)] public float mouthSmoothTime = 0.12f;

    [Header("Kinect Beam (Infrared Sensor Logic)")]
    [Tooltip("Horizontal pixel (0-511 for Kinect v2)")]
    public int depthX = 256;
    [Tooltip("Vertical pixel (0-423 for Kinect v2)")]
    public int depthY = 212;
    [Tooltip("Distance in millimeters (2000 = 2 meters)")]
    public float triggerDistance = 2000f;
    private bool isBlocked = false;

    private float h1Timer, h2Timer, h3Timer;
    private KinectZoneSoundManager KZSManager;
    private KinectManager kinect;
    private float[] audioDataBuffer = new float[1024];

    void Start()
    {
        KZSManager = FindObjectOfType<KinectZoneSoundManager>();
        kinect = KinectManager.Instance;

        // CRITICAL CHECK: Clip must be Decompress On Load
        if (performanceSource.clip != null && performanceSource.clip.loadType != AudioClipLoadType.DecompressOnLoad)
        {
            Debug.LogError($"<color=red>SETTING ERROR:</color> {performanceSource.clip.name} must be set to 'Decompress On Load' in the Inspector!");
        }
    }

    void Update()
    {
        // 1. MANUAL INPUT
        if (Keyboard.current.spaceKey.wasPressedThisFrame) PlayPerformance();

        // 2. KINECT: "Infrared Beam" Logic
        if (kinect != null && kinect.IsInitialized())
        {
            // Get the full depth map from the Kinect
            ushort[] depthMap = kinect.GetRawDepthMap();

            if (depthMap != null)
            {
                // Kinect v2 depth resolution is 512 x 424
                int width = 512;

                // Calculate the array index: (Y * Width) + X
                int index = (depthY * width) + depthX;

                // Ensure the index is within the array bounds
                if (index >= 0 && index < depthMap.Length)
                {
                    int currentDepth = depthMap[index];

                    // Check if something "distorts" the beam (closer than triggerDistance)
                    // Note: 0 usually means "too close to see" or "out of range"
                    if (currentDepth > 100 && currentDepth < triggerDistance)
                    {
                        if (!isBlocked)
                        {
                            isBlocked = true;
                            if (debugMode) Debug.Log($"Beam Tripped! Depth at [{depthX},{depthY}]: {currentDepth}mm");
                            PlayPerformance();
                        }
                    }
                    else
                    {
                        isBlocked = false;
                    }
                }
            }
        }

        // 3. IDLE / 4. LIP SYNC (Rest of your code remains the same)
        if (KZSManager != null && KZSManager.TimeNoInput > 60f) PlayIdle();
        HandleLipSync();
    }

    private void HandleLipSync()
    {
        if (performanceSource != null && performanceSource.isPlaying)
        {
            UpdateHead(head1Display, head1Normal, head1Talking, bruceChannel, performanceSource, ref h1Timer, "Bruce");
            UpdateHead(head2Display, head2Normal, head2Talking, doryChannel, performanceSource, ref h2Timer, "Dory");
            UpdateHead(head3Display, head3Normal, head3Talking, marlinChannel, performanceSource, ref h3Timer, "Marlin");
        }
        else if (idleSource != null && idleSource.isPlaying)
        {
            UpdateHead(head1Display, head1Normal, head1Talking, 0, idleSource, ref h1Timer, "Idle Bruce");
            UpdateHead(head2Display, head2Normal, head2Talking, -1, null, ref h2Timer, "");
            UpdateHead(head3Display, head3Normal, head3Talking, -1, null, ref h3Timer, "");
        }
        else
        {
            ResetFaces();
        }
    }

    private void UpdateHead(Image img, Sprite normal, Sprite talking, int channel, AudioSource source, ref float timer, string label)
    {
        if (img == null) return;

        if (source != null && source.clip != null && channel >= 0 && channel < source.clip.channels)
        {
            int channels = source.clip.channels;
            int pointer = source.timeSamples;

            if (pointer + 128 < source.clip.samples)
            {
                source.clip.GetData(audioDataBuffer, pointer);
                float sum = 0;
                for (int i = 0; i < 128; i++)
                {
                    float sample = audioDataBuffer[i * channels + channel];
                    sum += sample * sample;
                }
                float volume = Mathf.Sqrt(sum / 128);

                if (volume > sensitivity) timer = mouthSmoothTime;
            }
        }

        if (timer > 0)
        {
            img.sprite = talking;
            timer -= Time.deltaTime;
        }
        else
        {
            img.sprite = normal;
        }
    }

    private void ResetFaces()
    {
        if (head1Display) head1Display.sprite = head1Normal;
        if (head2Display) head2Display.sprite = head2Normal;
        if (head3Display) head3Display.sprite = head3Normal;
        h1Timer = h2Timer = h3Timer = 0;
    }

    void PlayPerformance()
    {
        if (idleSource == null) return;

        PlayIdle();
        if (debugMode) Debug.Log("<color=cyan>Performance Started.</color>");
    }

    void PlayIdle()
    {
        if (idleClips == null || idleClips.Length == 0) return;

        // Check if the source is already busy
        if (idleSource.isPlaying) return;

        // If we got here, nothing is playing, so let's start a new one
        if (KZSManager != null) 
        {
            KZSManager.ResetAudio();
            KZSManager.TimeNoInput = 0f;
        }

        idleSource.clip = idleClips[Random.Range(0, idleClips.Length)];
        idleSource.Play();
    }
}