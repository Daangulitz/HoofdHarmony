using UnityEngine;
using UnityEngine.UI;

public class MannequinHead : MonoBehaviour
{
    [Header("Visuals")]
    public Image faceDisplay;
    public Sprite normalSprite;
    public Sprite talkingSprite;

    [Header("Audio to Watch")]
    public AudioSource myAudioSource;
    [Range(0, 1)] public int channel = 0; // 0=Left, 1=Right
    public float threshold = 0.02f;

    private float[] samples = new float[256];

    void Update()
    {
        // If no audio is playing, stay normal
        if (myAudioSource == null || !myAudioSource.isPlaying)
        {
            faceDisplay.sprite = normalSprite;
            return;
        }

        // Check volume
        myAudioSource.GetOutputData(samples, channel);
        float sum = 0;
        for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
        float rms = Mathf.Sqrt(sum / samples.Length);

        // Swap sprite based on volume
        faceDisplay.sprite = (rms > threshold) ? talkingSprite : normalSprite;
    }
}