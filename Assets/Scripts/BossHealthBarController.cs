using UnityEngine;
using UnityEngine.UI; // Add this for UI components

public class BossHealthBarController : MonoBehaviour
{
    [Header("Health Settings")]
    public float luongMauHienTai;
    public float luongMauToiDa = 12000;
    
    [Header("UI References")]
    [SerializeField] private Slider healthSlider; // Replace BossHealthBar with Slider
    [SerializeField] private bool smoothHealthBar = true;
    [SerializeField] private float smoothSpeed = 5f;
    
    [Header("Hit Feedback")]
    [SerializeField] private bool showHitFeedback = true;
    [SerializeField] private float hitFlashDuration = 0.15f;
    [SerializeField] private Color hitColor = Color.red;
    
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isFlashing = false;
    private float targetSliderValue;
    
    void Start()
    {
        luongMauHienTai = luongMauToiDa; // Start with full health
        
        // Initialize the slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = luongMauToiDa;
            healthSlider.value = luongMauHienTai;
            targetSliderValue = luongMauHienTai;
        }
        else
        {
            Debug.LogError("Health Slider reference is missing! Assign it in the inspector.");
        }
        
        // Cache all renderers for hit effect
        // ...existing code...
        if (showHitFeedback)
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = renderers[i].material.color;
                }
            }
        }
    }
    
    void Update()
    {
        // Smooth health bar transition
        if (smoothHealthBar && healthSlider != null)
        {
            if (healthSlider.value != targetSliderValue)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetSliderValue, Time.deltaTime * smoothSpeed);
            }
        }
    }

    // Method to handle taking damage
    public void TakeDamage(float damage)
    {
        Debug.Log($"Boss taking {damage} damage. Current health: {luongMauHienTai}");
        
        luongMauHienTai -= damage;
        
        // Ensure health doesn't go below zero
        if (luongMauHienTai < 0)
        {
            luongMauHienTai = 0;
        }
        
        // Update the boss health bar
        if (healthSlider != null)
        {
            if (smoothHealthBar)
            {
                targetSliderValue = luongMauHienTai;
            }
            else
            {
                healthSlider.value = luongMauHienTai;
            }
        }
        
        // Show hit feedback
        // ...existing code...
        if (showHitFeedback && !isFlashing && renderers != null && renderers.Length > 0)
        {
            StartCoroutine(FlashOnHit());
        }
        
        // Check if boss is defeated
        if (luongMauHienTai <= 0)
        {
            Debug.Log("Boss defeated!");
            // You can add boss defeat logic here
        }
    }
    
    // Test function to directly damage boss from Inspector
    [ContextMenu("Test Damage 100")]
    public void TestDamage()
    {
        TakeDamage(100f);
        Debug.Log($"Test damage applied. Boss health: {luongMauHienTai}/{luongMauToiDa}");
    }
    
    private System.Collections.IEnumerator FlashOnHit()
    {
        // ...existing code...
        isFlashing = true;
        
        // Change to hit color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = hitColor;
            }
        }
        
        // Wait for flash duration
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
        
        isFlashing = false;
    }
}