using UnityEngine;
using UnityEngine.UI; // Add this for UI components
using UnityEngine.SceneManagement;

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
    
    [Header("Game Controls")]
    [SerializeField] private bool autoLoadWinScene = true;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private int winSceneIndex = 4;
    
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isFlashing = false;
    private float targetSliderValue;
    private bool isDead = false;
    
    // Reference to GameStateManager
    private GameStateManager gameStateManager;
    
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
        if (showHitFeedback)
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = renderers[i].material.color;
                }
            }
        }
        
        // Try to find GameStateManager using multiple methods
        FindGameStateManager();
    }
    
    private void FindGameStateManager()
    {
        // Method 1: Using the Instance property
        gameStateManager = GameStateManager.Instance;
        
        // Method 2: Using FindObjectOfType if Method 1 failed
        if (gameStateManager == null)
        {
            gameStateManager = FindObjectOfType<GameStateManager>();
        }
        
        // Method 3: Using static accessor if implemented
        if (gameStateManager == null && typeof(GameStateManager).GetMethod("GetInstance") != null)
        {
            gameStateManager = GameStateManager.GetInstance();
        }
        
        // Log result
        if (gameStateManager == null)
        {
            Debug.LogWarning("GameStateManager not found. Win condition may not trigger correctly.");
        }
        else
        {
            Debug.Log("GameStateManager found and connected to BossHealthBarController");
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
        
        // Try to find GameStateManager if not found initially and we're not dead
        if (gameStateManager == null && !isDead)
        {
            // Only search occasionally to avoid performance hit
            if (Time.frameCount % 60 == 0)
            {
                FindGameStateManager();
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
        if (showHitFeedback && !isFlashing && renderers != null && renderers.Length > 0)
        {
            StartCoroutine(FlashOnHit());
        }
        
        // Check if boss is defeated
        if (luongMauHienTai <= 0 && !isDead)
        {
            isDead = true;
            Debug.Log("Boss defeated! Health reached zero.");
            
            // Notify GameStateManager if available
            if (gameStateManager != null)
            {
                gameStateManager.NotifyBossDefeated();
            }
            
            // Start death sequence
            StartCoroutine(DestroyBossAfterDelay(destroyDelay));
        }
    }
    
    // Thêm coroutine để xóa boss sau một khoảng thời gian
    private System.Collections.IEnumerator DestroyBossAfterDelay(float delay)
    {
        Debug.Log($"Boss will be destroyed after {delay} seconds");
        
        // Đợi một khoảng thời gian để hiển thị hiệu ứng hoặc animation (nếu có)
        yield return new WaitForSeconds(delay);
        
        // Ẩn thanh máu của boss trước khi xóa boss
        if (healthSlider != null)
        {
            // Tìm GameObject cha của thanh máu
            GameObject healthBarObject = healthSlider.gameObject;
            while (healthBarObject != null && healthBarObject.GetComponent<Canvas>() == null)
            {
                healthBarObject = healthBarObject.transform.parent?.gameObject;
            }
            
            // Nếu tìm thấy Canvas chứa thanh máu, ẩn nó
            if (healthBarObject != null && healthBarObject.GetComponent<Canvas>() != null)
            {
                healthBarObject.SetActive(false);
                Debug.Log("Boss health bar UI has been hidden");
            }
            else
            {
                // Nếu không tìm thấy Canvas, ẩn trực tiếp thanh máu
                Transform parent = healthSlider.transform.parent;
                if (parent != null)
                {
                    parent.gameObject.SetActive(false);
                    Debug.Log("Boss health bar parent has been hidden");
                }
                else
                {
                    healthSlider.gameObject.SetActive(false);
                    Debug.Log("Boss health slider has been hidden");
                }
            }
        }
        
        // Try to notify GameStateManager one more time
        if (gameStateManager != null)
        {
            gameStateManager.NotifyBossDefeated();
        }
        else if (autoLoadWinScene)
        {
            // Direct scene load as fallback if GameStateManager isn't available
            Debug.Log("No GameStateManager found. Loading win scene directly...");
            SceneManager.LoadScene(winSceneIndex);
        }
        
        // Xóa đối tượng boss
        Debug.Log($"Boss object ({gameObject.name}) is now being destroyed!");
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        Debug.Log($"Boss OnDestroy event triggered for {gameObject.name}");
        
        // Try to notify GameStateManager one final time
        if (gameStateManager != null)
        {
            Debug.Log("Final boss destruction notification to GameStateManager");
            gameStateManager.NotifyBossDefeated();
        }
        else if (isDead && autoLoadWinScene)
        {
            // Direct scene load as final fallback
            Debug.Log("Attempting direct scene load from OnDestroy");
            SceneManager.LoadScene(winSceneIndex);
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
            if (renderers[i] != null && renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = hitColor;
            }
        }
        
        // Wait for flash duration
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
        
        isFlashing = false;
    }
    
    // Direct method to force scene transition (for debugging)
    [ContextMenu("Force Win Scene")]
    public void ForceWinScene()
    {
        if (gameStateManager != null)
        {
            gameStateManager.ForceWin();
        }
        else
        {
            SceneManager.LoadScene(winSceneIndex);
        }
    }
}