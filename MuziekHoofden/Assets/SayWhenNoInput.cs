using UnityEngine;
using UnityEngine.InputSystem;
using OscJack; // Requires OscJack Unity Package installed

public class QuoteManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueScene
    {
        public string sceneName;
        public AudioClip head1Clip;
        public AudioClip head2Clip;
        public AudioClip head3Clip;
    }

    [Header("Assign the Face Audio Sources")]
    [SerializeField] private AudioSource face1;
    [SerializeField] private AudioSource face2;
    [SerializeField] private AudioSource face3;

    [Header("Your Movie Quotes")]
    public DialogueScene[] library;
    
    [Header("Tripwire Settings (OSC from Pi)")]
    [Tooltip("Must match the port in your Python script")]
    public int oscPort = 5005;
    [Tooltip("Distance in meters to trigger the heads (e.g., 1.5 = 1.5 meters)")]
    public float triggerDistanceMeters = 1.5f;
    [Tooltip("Ignore anything closer than this (e.g., 0.5m) to avoid noise")]
    public float minDistanceMeters = 0.5f;

    [Header("Debug Info")]
    public bool debugMode = true;
    [ReadOnly] public float currentKinectDepth = 0f; // View this in Inspector to test

    private bool isBeamBlocked = false;
    private OscServer server;

    void Start()
    {
        // Initialize the OscJack Server
        // GetSharedServer ensures we don't accidentally open the same port twice
        server = OscMaster.GetSharedServer(oscPort);

        // Bind the address sent by your Raspberry Pi Python script
        server.MessageDispatcher.AddCallback("/kinect/depth", OnReceiveDepth);

        if (debugMode) Debug.Log($"<color=green>OSC Receiver Active on Port {oscPort}</color>");
    }

    // This runs whenever a new packet arrives from the Pi
    private void OnReceiveDepth(string address, OscDataHandle data)
    {
        // Get the float value sent from the Python script
        currentKinectDepth = data.GetElementAsFloat(0);
    }

    void Update()
    {
        // Manual trigger for testing
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayRandomQuote();
        }
        
        // Tripwire Logic
        if (!IsAnythingPlaying())
        {
            CheckTripwire();
        }
    }

    private bool IsAnythingPlaying()
    {
        bool f1Busy = face1 != null && face1.isPlaying;
        bool f2Busy = face2 != null && face2.isPlaying;
        bool f3Busy = face3 != null && face3.isPlaying;

        return f1Busy || f2Busy || f3Busy;
    }
    
    private void CheckTripwire()
    {
        // Check if current distance is within our 'Tripwire' zone
        if (currentKinectDepth > minDistanceMeters && currentKinectDepth < triggerDistanceMeters)
        {
            if (!isBeamBlocked)
            {
                isBeamBlocked = true;
                if (debugMode) Debug.Log($"<b>Tripwire Triggered!</b> Distance: {currentKinectDepth}m");
                PlayRandomQuote();
            }
        }
        else
        {
            // Reset the tripwire so it can be triggered again
            isBeamBlocked = false;
        }
    }

    [ContextMenu("Play Random Quote")]
    public void PlayRandomQuote()
    {
        if (IsAnythingPlaying())
        {
            if (debugMode) Debug.Log("Heads are busy talking, skipping trigger.");
            return;
        }

        if (library.Length == 0) return;

        int randomIndex = Random.Range(0, library.Length);
        PlayQuote(randomIndex);
    }

    public void PlayQuote(int index)
    {
        if (index < 0 || index >= library.Length) return;

        DialogueScene quote = library[index];

        HandleHeadPlay(face1, quote.head1Clip);
        HandleHeadPlay(face2, quote.head2Clip);
        HandleHeadPlay(face3, quote.head3Clip);
    }

    private void HandleHeadPlay(AudioSource face, AudioClip clip)
    {
        if (face != null && clip != null)
        {
            face.clip = clip;
            face.Play();
        }
    }

    void OnDestroy()
    {
        // Clean up the listener when the game stops or the object is destroyed
        if (server != null)
        {
            server.MessageDispatcher.RemoveCallback("/kinect/depth", OnReceiveDepth);
        }
    }
}

// Simple attribute to make the float viewable but not editable in Inspector
public class ReadOnlyAttribute : PropertyAttribute { }