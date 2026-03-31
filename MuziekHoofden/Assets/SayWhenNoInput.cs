using UnityEngine;
using UnityEngine.InputSystem;
using OscJack;
using System.Collections.Generic;

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

    [Header("Assign Face Audio Sources")]
    [SerializeField] private AudioSource face1;
    [SerializeField] private AudioSource face2;
    [SerializeField] private AudioSource face3;

    [Header("Movie Quotes Library")]
    public DialogueScene[] library;
    [Tooltip("How many unique quotes play before repeating?")]
    public int historyLimit = 3;
    private List<int> lastPlayedIndices = new List<int>();

    [Header("Eye Rotation (X-Axis)")]
    [SerializeField] private Transform[] pupils; 
    [Tooltip("How many degrees the eyes rotate left/right")]
    public float maxRotationAngle = 25f;
    [Range(1f, 20f)] public float eyeSmoothSpeed = 8f;

    [Header("Tripwire & OSC Settings")]
    public int oscPort = 5005;
    public float triggerDistanceMeters = 1.5f;
    public float minDistanceMeters = 0.5f;
    public bool debugMode = true;

    // OSC Data Variables
    private float currentKinectDepth = 0f;
    private float targetXRotation = 0f; // -1.0 to 1.0
    private float smoothedX = 0f;
    private bool isBeamBlocked = false;
    private OscServer server;

    void Start()
    {
        // Setup OSC Server
        server = OscMaster.GetSharedServer(oscPort);
        
        // Listen for depth (Tripwire) and xpos (Eye tracking)
        server.MessageDispatcher.AddCallback("/kinect/depth", OnReceiveDepth);
        server.MessageDispatcher.AddCallback("/kinect/xpos", OnReceiveX);

        if (library.Length > 0 && historyLimit >= library.Length)
            historyLimit = library.Length - 1;
    }

    private void OnReceiveDepth(string address, OscDataHandle data) => currentKinectDepth = data.GetElementAsFloat(0);
    private void OnReceiveX(string address, OscDataHandle data) => targetXRotation = data.GetElementAsFloat(0);

    void Update()
    {
        // 1. Handle Eye Rotation
        RotateEyes();

        // 2. Manual Test Trigger
        if (Keyboard.current.spaceKey.wasPressedThisFrame) PlayRandomQuote();

        // 3. Tripwire Logic
        if (!IsAnythingPlaying())
        {
            CheckTripwire();
        }
    }

    private void RotateEyes()
    {
        if (pupils == null || pupils.Length == 0) return;

        // Smooth the input value
        smoothedX = Mathf.Lerp(smoothedX, targetXRotation, Time.deltaTime * eyeSmoothSpeed);

        // Calculate rotation (mapping -1/1 to -maxAngle/maxAngle)
        float rotationY = smoothedX * maxRotationAngle;

        foreach (Transform pupil in pupils)
        {
            if (pupil != null)
            {
                // We rotate around the Y-axis to move the look horizontally
                pupil.localEulerAngles = new Vector3(rotationY, 0, 90);
            }
        }
    }

    private bool IsAnythingPlaying()
    {
        return (face1 != null && face1.isPlaying) || 
               (face2 != null && face2.isPlaying) || 
               (face3 != null && face3.isPlaying);
    }

    private void CheckTripwire()
    {
        if (currentKinectDepth > minDistanceMeters && currentKinectDepth < triggerDistanceMeters)
        {
            if (!isBeamBlocked)
            {
                isBeamBlocked = true;
                if (debugMode) Debug.Log($"Tripwire! Person at: {currentKinectDepth}m");
                PlayRandomQuote();
            }
        }
        else
        {
            isBeamBlocked = false;
        }
    }

    [ContextMenu("Play Random Quote")]
    public void PlayRandomQuote()
    {
        if (IsAnythingPlaying() || library.Length == 0) return;

        int randomIndex = GetRandomIndex();
        
        lastPlayedIndices.Add(randomIndex);
        if (lastPlayedIndices.Count > historyLimit) lastPlayedIndices.RemoveAt(0);

        PlayQuote(randomIndex);
    }

    private int GetRandomIndex()
    {
        if (library.Length <= 1) return 0;
        int index;
        int attempts = 0;
        do {
            index = Random.Range(0, library.Length);
            attempts++;
        } while (lastPlayedIndices.Contains(index) && attempts < 100);
        return index;
    }

    public void PlayQuote(int index)
    {
        DialogueScene quote = library[index];
        if (debugMode) Debug.Log($"Playing: {quote.sceneName}");

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
        if (server != null)
        {
            server.MessageDispatcher.RemoveCallback("/kinect/depth", OnReceiveDepth);
            server.MessageDispatcher.AddCallback("/kinect/xpos", OnReceiveX);
        }
    }
}