using System;

public interface IGestureNotifier
{
    // Implement this event in your Kinect gesture script and raise it with the gesture name
    event Action<string> OnGestureDetected;
}