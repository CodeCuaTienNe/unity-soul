using UnityEngine;

public class BossHealthBarController : MonoBehaviour
{
    public BossHealthBar bossHealthBar;
    public float luongMauHienTai;
    public float luongMauToiDa = 12000;
    
    [Header("Hit Feedback")]
    [SerializeField] private bool showHitFeedback = true;
    [SerializeField] private float hitFlashDuration = 0.15f;
    [SerializeField] private Color hitColor = Color.red;
    
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isFlashing = false;
    
    void Start()
    {
        luongMauHienTai = luongMauToiDa; // Start with full health
        if (bossHealthBar != null)
        {
            bossHealthBar.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
        }
        else
        {
            Debug.LogError("Boss Health Bar reference is missing! Assign it in the inspector.");
        }
        
        // Cache all renderers for hit effect
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
        if (bossHealthBar != null)
        {
            bossHealthBar.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
        }
        
        // Show hit feedback
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