using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Assign the Face Scripts")]
    [SerializeField] private AudioSource face1;
    [SerializeField] private AudioSource face2;
    [SerializeField] private AudioSource face3;

    [Header("Your Movie Quotes")]
    public DialogueScene[] library;
    
    [Header("Kinect Tripwire Settings")]
    public int depthX = 256;
    public int depthY = 212;
    public float triggerDistance = 1500f;
    private bool isBeamBlocked = false;

    [Header("Lip Sync Settings")]
    public bool debugMode = true;

    private KinectManager kinect;

    void Start()
    {
        kinect = KinectManager.Instance;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayRandomQuote();
        }
        
        if (!IsAnythingPlaying())
        {
            CheckKinectBeam();
        }
    }

    // New helper function to check if the performance is still ongoing
    private bool IsAnythingPlaying()
    {
        bool f1Busy = face1 != null && face1.isPlaying;
        bool f2Busy = face2 != null && face2.isPlaying;
        bool f3Busy = face3 != null && face3.isPlaying;

        return f1Busy || f2Busy || f3Busy;
    }
    
    private void CheckKinectBeam()
    {
        if (kinect == null || !kinect.IsInitialized()) return;

        ushort[] depthMap = kinect.GetRawDepthMap();
        if (depthMap == null) return;

        int index = (depthY * 512) + depthX;
        if (index >= 0 && index < depthMap.Length)
        {
            int currentDepth = depthMap[index];

            if (currentDepth > 500 && currentDepth < triggerDistance)
            {
                if (!isBeamBlocked)
                {
                    isBeamBlocked = true;
                    if (debugMode) Debug.Log("Beam Tripped! Attempting to play quote.");
                    PlayRandomQuote();
                }
            }
            else
            {
                isBeamBlocked = false;
            }
        }
    }

    [ContextMenu("Play Random Quote")]
    public void PlayRandomQuote()
    {
        if (IsAnythingPlaying())
        {
            if (debugMode) Debug.Log("Can't play yet: Heads are still talking.");
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
        if (face != null)
        {
            face.clip = clip;
            if (clip != null)
            {
                face.Play();
            }
        }
    }
}