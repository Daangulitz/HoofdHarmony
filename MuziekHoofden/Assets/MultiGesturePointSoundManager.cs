using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MultiGesturePointSoundManager - Handles multiple gesture types with different sounds
/// This is an enhanced version that combines pointing, swipes, and other gestures
/// with different sounds for each gesture type
/// </summary>
public class MultiGesturePointSoundManager : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [System.Serializable]
    public class GestureSound
    {
        public KinectGestures.Gestures gesture;
        public AudioClip soundClip;
    }

    [Header("Audio Setup")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Gesture Sounds")]
    [SerializeField] private List<GestureSound> gestureSounds = new List<GestureSound>();

    private KinectManager kinectManager;
    private Dictionary<KinectGestures.Gestures, AudioClip> gestureSoundMap;

    void Start()
    {
        kinectManager = KinectManager.Instance;
        
        if (kinectManager == null)
        {
            Debug.LogError("MultiGesturePointSoundManager: KinectManager not found!");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("MultiGesturePointSoundManager: No AudioSource found!");
            }
        }

        // Build the gesture-to-sound map
        BuildGestureSoundMap();
    }

    private void BuildGestureSoundMap()
    {
        gestureSoundMap = new Dictionary<KinectGestures.Gestures, AudioClip>();
        
        foreach (GestureSound gs in gestureSounds)
        {
            if (gs.soundClip != null)
            {
                gestureSoundMap[gs.gesture] = gs.soundClip;
            }
        }
    }
    

    public void UserDetected(long userId, int userIndex)
    {
        // Register all gestures that have sounds
        foreach (GestureSound gs in gestureSounds)
        {
            kinectManager.DetectGesture(userId, gs.gesture);
            Debug.Log($"Detecting gesture: {gs.gesture}");
        }
    }

    public void UserLost(long userId, int userIndex)
    {
        Debug.Log("User lost");
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // Optional: Show progress during gestures
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        Debug.Log($"Gesture completed: {gesture}");
        
        // Check if we have a sound for this gesture
        if (gestureSoundMap.ContainsKey(gesture) && audioSource != null)
        {
            AudioClip clip = gestureSoundMap[gesture];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                Debug.Log($"Playing sound for gesture: {gesture}");
            }
        }
        
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint)
    {
        Debug.Log($"Gesture cancelled: {gesture}");
        return true;
    }
}
