using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SayWhenNoInput : MonoBehaviour
{
    [Header("Head 1 (Jens)")]
    public Image head1Display;
    public Sprite head1Normal, head1Talking;
    public int bruceChannel = 0; // Use 0 for 3-track file, or 2 for 5-track file

    [Header("Head 2 (Marijn)")]
    public Image head2Display;
    public Sprite head2Normal, head2Talking;
    public int doryChannel = 1; // Use 1 for 3-track file, or 3 for 5-track file

    [Header("Head 3 (Daan)")]
    public Image head3Display;
    public Sprite head3Normal, head3Talking;
    public int marlinChannel = 2; // Use 2 for 3-track file, or 4 for 5-track file

    [Header("Audio Sources")]
    public AudioSource performanceSource; 
    public AudioSource idleSource;
    public AudioClip itsAWitch;
    public AudioClip[] idleClips;

    [Header("Settings")]
    public bool debugMode = true; 
    [Range(0.001f, 0.2f)] public float sensitivity = 0.01f;
    [Range(0.01f, 0.5f)] public float mouthSmoothTime = 0.12f;

    private float h1Timer, h2Timer, h3Timer;
    private KinectZoneSoundManager KZSManager;
    private KinectManager kinect;
    private bool userDetected = false;
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
        // 1. INPUT: Spacebar to trigger performance manually
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayIdle();
        }

        // 2. KINECT: Trigger performance if person enters
        if (kinect != null)
        {
            bool detected = kinect.IsUserDetected();
            if (detected && !userDetected)
            {
                userDetected = true;
                PlayPerformance();
            }
            else if (!detected)
            {
                userDetected = false;
            }
        }

        // 3. IDLE: Auto-trigger idle sounds
        if (KZSManager != null && KZSManager.TimeNoInput > 60f)
        {
            PlayIdle();
        }

        // 4. LIP SYNC: Run every frame while audio is playing
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
            // Idle usually only moves Bruce
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
            
            // Read raw data from the file
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

                // Check volume levels in console
                if (debugMode && volume > 0.001f)
                {
                    Debug.Log($"{label} (Ch {channel}) Volume: {volume}");
                }

                if (volume > sensitivity) timer = mouthSmoothTime;
            }
        }

        // Apply Sprite Swap
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
        if (performanceSource == null) return;

        performanceSource.Stop(); 
        
        // --- FORCE OUTPUT TO CHANNEL 1 ONLY ---
        performanceSource.spatialBlend = 0f; // 2D Mode
        performanceSource.panStereo = -1f;   // Pan 100% Left

        if (idleSource != null) 
        {
            idleSource.PlayOneShot(itsAWitch);
        }
        
        performanceSource.Play();
        Debug.Log("<color=cyan>Audio Sync:</color> Playing Bruce on Left Speaker. Script reading all 5 channels.");
    }

    void PlayIdle()
    {
        if (idleClips.Length == 0) return;
        if (KZSManager != null) KZSManager.ResetAudio();
        
        idleSource.clip = idleClips[Random.Range(0, idleClips.Length)];
        idleSource.Play();
        
        if (KZSManager != null) KZSManager.TimeNoInput = 0f;
    }
}