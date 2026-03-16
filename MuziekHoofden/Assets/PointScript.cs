using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

// We add the interface here so this script can "hear" gestures from the KinectManager
public class KinectZoneSoundManager : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [Header("Audio Components")] 
    [SerializeField] private AudioSource LeftaudioSource;
    [SerializeField] private AudioSource RightaudioSource;
    
    [Header("UI Indicators")]
    [SerializeField] private Image RightInducator;
    [SerializeField] private Image LeftInducator;
    [SerializeField] private Sprite RightTexturenormal;
    [SerializeField] private Sprite RightTextureActive;
    [SerializeField] private Sprite LeftTexturenormal;
    [SerializeField] private Sprite LeftTextureActive;

    [Header("Settings")]
    public bool IsPlayedLeft = false;
    public bool IsPlayedRight = false;
    [Tooltip("0 is dead center. Negative is left, positive is right.")]
    public float centerThreshold = 0f;

    private KinectManager kinectManager;
    
    public float TimeNoInput = 0;
    
    // Internal flags to track when a gesture just happened
    private bool swipedLeft = false;
    private bool swipedRight = false;
    private bool swipedUp = false;

    void Start()
    {
        kinectManager = KinectManager.Instance;
        
        if (kinectManager == null)
        {
            Debug.LogError("KinectZoneSoundManager: KinectManager not found!");
        }
    }

    void Update()
    {
        if (kinectManager == null || !kinectManager.IsUserDetected())
        {
            TimeNoInput = Time.deltaTime + TimeNoInput;
            return; 
        }

        long userId = kinectManager.GetPrimaryUserID();
        
        Vector3 handPos = kinectManager.GetJointPosition(userId, (int)KinectInterop.JointType.HandRight);

        HandleGestureLogic(handPos);
    }

    private void HandleGestureLogic(Vector3 handPos)
    {
        // --- SWIPE LEFT ---
        if (swipedLeft)
        {
            if (handPos.x < centerThreshold) LeftaudioSource.pitch -= 0.2f;
            else RightaudioSource.pitch -= 0.2f;
            swipedLeft = false; // Reset flag
        }

        // --- SWIPE RIGHT ---
        if (swipedRight)
        {
            if (handPos.x < centerThreshold) LeftaudioSource.pitch += 0.2f;
            else RightaudioSource.pitch += 0.2f;
            swipedRight = false; // Reset flag
        }

        // --- SWIPE UP ---
        if (swipedUp)
        {
            if (handPos.x < centerThreshold)
            {
                if (!IsPlayedLeft)
                {
                    LeftaudioSource.Play(); 
                    IsPlayedLeft = true; 
                    LeftInducator.sprite = LeftTextureActive;
                }
                else
                {
                    LeftaudioSource.Stop(); 
                    IsPlayedLeft = false;
                    LeftInducator.sprite = LeftTexturenormal;
                }
            }
            else
            {
                if (!IsPlayedRight)
                {
                    RightaudioSource.Play();
                    RightInducator.sprite = RightTextureActive;
                    IsPlayedRight = true;
                }
                else
                {
                    RightaudioSource.Stop();
                    RightInducator.sprite = RightTexturenormal;
                    IsPlayedRight = false;
                }
            }
            swipedUp = false; // Reset flag
        }
    }
    
    public void ResetAudio()
    {
        LeftaudioSource.Stop();
        RightaudioSource.Stop();
        IsPlayedLeft = false;
        IsPlayedRight = false;
        LeftInducator.sprite = LeftTexturenormal;
        RightInducator.sprite = RightTexturenormal;
    }

    // --- KINECT GESTURE INTERFACE METHODS ---
    // These are called automatically by the KinectManager

    public void UserDetected(long userId, int userIndex)
    {
        // Tell KinectManager to detect these specific gestures for this user
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeUp);
    }

    public void UserLost(long userId, int userIndex) { }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos) { }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        // When a gesture finishes, set our flags to true
        if (gesture == KinectGestures.Gestures.SwipeLeft) swipedLeft = true;
        if (gesture == KinectGestures.Gestures.SwipeRight) swipedRight = true;
        if (gesture == KinectGestures.Gestures.SwipeUp) swipedUp = true;
        
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint)
    {
        return true;
    }
}