using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Image _healBar;
    
    [Header("Audio")]
    [SerializeField] private AudioClip healthUpdateSound;
    [SerializeField] private float soundVolume = 1.0f;
    private AudioSource audioSource;
    
    private float lastHealthRatio = 1.0f; // Track last health value to detect changes

    private void Awake()
    {
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void capNhatThanhMau(float luongMauHienTai, float luongMauToiDa)
    {
        float ratioValue = luongMauHienTai / luongMauToiDa;
        
        // Check if health value has changed significantly
        if (Mathf.Abs(ratioValue - lastHealthRatio) > 0.001f)
        {
            // Play sound when health updates
            PlayHealthUpdateSound();
            lastHealthRatio = ratioValue;
        }
        
        _healBar.fillAmount = ratioValue;
    }
    
    private void PlayHealthUpdateSound()
    {
        // Verify we have both an audio source and clip
        if (audioSource != null && healthUpdateSound != null)
        {
            audioSource.PlayOneShot(healthUpdateSound, soundVolume);
        }
    }
    
    // For testing in the editor
    [ContextMenu("Test Health Bar 50%")]
    public void TestHealthBar()
    {
        capNhatThanhMau(50, 100);
    }

    [ContextMenu("Test Health Bar 100%")]
    public void TestHealthBarFull()
    {
        capNhatThanhMau(100, 100);
    }
}
