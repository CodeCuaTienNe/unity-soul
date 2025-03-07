using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeController : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private float volumeChangeAmount = 0.1f;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;
    
    [Header("Optional UI Elements")]
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // Try to find the TextMeshPro component if it's not assigned
        if (volumeText == null)
        {
            volumeText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        UpdateUI();
    }

    public void IncreaseVolume()
    {
        // Ensure cursor is visible when interacting with UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioListener.volume = Mathf.Clamp(AudioListener.volume + volumeChangeAmount, minVolume, maxVolume);
        UpdateUI();
        Debug.Log($"Volume increased to: {AudioListener.volume}");
    }

    public void DecreaseVolume()
    {
        // Ensure cursor is visible when interacting with UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioListener.volume = Mathf.Clamp(AudioListener.volume - volumeChangeAmount, minVolume, maxVolume);
        UpdateUI();
        Debug.Log($"Volume decreased to: {AudioListener.volume}");
    }

    private void UpdateUI()
    {
        if (volumeText != null)
        {
            volumeText.text = $"Volume: {(AudioListener.volume * 100):F0}%";
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
        }
    }
}
