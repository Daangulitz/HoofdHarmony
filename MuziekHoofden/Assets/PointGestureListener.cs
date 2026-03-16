using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// PointGestureListener detects when users point and plays different sounds based on which hand they use
/// Attach this script to a GameObject and configure the audio sources and UI indicators
/// </summary>
public class PointGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [Header("Audio Components")] 
    [SerializeField] private AudioSource LeftPointAudioSource;
    [SerializeField] private AudioSource RightPointAudioSource;
    
    [Header("UI Indicators")]
    [SerializeField] private Image RightPointIndicator;
    [SerializeField] private Image LeftPointIndicator;
    [SerializeField] private Sprite IndicatorNormal;
    [SerializeField] private Sprite IndicatorActive;

    [Header("Settings")]
    [Tooltip("Duration to show the active indicator (in seconds)")]
    public float indicatorDisplayDuration = 1.0f;

    private KinectManager kinectManager;
    
    // Internal flags to track when a gesture just happened
    private bool pointedRight = false;
    private bool pointedLeft = false;
    
    // Track when the gesture was detected for UI feedback
    private float rightPointTime = 0f;
    private float leftPointTime = 0f;

    void Start()
    {
        kinectManager = KinectManager.Instance;
        
        if (kinectManager == null)
        {
            Debug.LogError("PointGestureListener: KinectManager not found!");
        }
        
        // Initialize indicators to normal state
        if (RightPointIndicator != null && IndicatorNormal != null)
        {
            RightPointIndicator.sprite = IndicatorNormal;
        }
        if (LeftPointIndicator != null && IndicatorNormal != null)
        {
            LeftPointIndicator.sprite = IndicatorNormal;
        }
    }

    void Update()
    {
        // Handle UI indicator timeout for right point
        if (RightPointIndicator != null && IndicatorActive != null && IndicatorNormal != null)
        {
            if (Time.time - rightPointTime > indicatorDisplayDuration && RightPointIndicator.sprite == IndicatorActive)
            {
                RightPointIndicator.sprite = IndicatorNormal;
            }
        }
        
        // Handle UI indicator timeout for left point
        if (LeftPointIndicator != null && IndicatorActive != null && IndicatorNormal != null)
        {
            if (Time.time - leftPointTime > indicatorDisplayDuration && LeftPointIndicator.sprite == IndicatorActive)
            {
                LeftPointIndicator.sprite = IndicatorNormal;
            }
        }
    }

    // --- KINECT GESTURE INTERFACE METHODS ---
    // These are called automatically by the KinectManager

    public void UserDetected(long userId, int userIndex)
    {
        // Tell KinectManager to detect pointing gestures for this user
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.PointRight);
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.PointLeft);
        
        Debug.Log("User detected. Point gesture detection started.");
    }

    public void UserLost(long userId, int userIndex)
    {
        Debug.Log("User lost. Point gesture detection stopped.");
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // Optional: Show progress for pointing gesture
        // This is called while the gesture is being performed
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        // When a gesture finishes, set our flags to true and play sounds
        if (gesture == KinectGestures.Gestures.PointRight)
        {
            pointedRight = true;
            HandleRightPointGesture();
            return true;
        }
        
        if (gesture == KinectGestures.Gestures.PointLeft)
        {
            pointedLeft = true;
            HandleLeftPointGesture();
            return true;
        }
        
        return false;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, 
                                  KinectInterop.JointType joint)
    {
        return true;
    }

    // Handle right point gesture action
    private void HandleRightPointGesture()
    {
        Debug.Log("RIGHT POINT GESTURE DETECTED!");
        
        // Play sound if audio source is assigned
        if (RightPointAudioSource != null)
        {
            RightPointAudioSource.PlayOneShot(RightPointAudioSource.clip);
        }
        
        // Update UI indicator
        if (RightPointIndicator != null && IndicatorActive != null)
        {
            RightPointIndicator.sprite = IndicatorActive;
            rightPointTime = Time.time;
        }
    }

    // Handle left point gesture action
    private void HandleLeftPointGesture()
    {
        Debug.Log("LEFT POINT GESTURE DETECTED!");
        
        // Play sound if audio source is assigned
        if (LeftPointAudioSource != null)
        {
            LeftPointAudioSource.PlayOneShot(LeftPointAudioSource.clip);
        }
        
        // Update UI indicator
        if (LeftPointIndicator != null && IndicatorActive != null)
        {
            LeftPointIndicator.sprite = IndicatorActive;
            leftPointTime = Time.time;
        }
    }
}
