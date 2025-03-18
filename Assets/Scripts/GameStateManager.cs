using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    [Header("Scene Information")]
    [SerializeField] private int winSceneIndex = 4;
    [SerializeField] private int loseSceneIndex = 5;
    [SerializeField] private float transitionDelay = 1.5f; // Time before scene transition

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject boss;

    private bool isTransitioning = false;
    private PlayerHealthController playerHealth;
    private BossHealthBarController bossHealth;

    // Make this a singleton to ensure we only have one instance
    public static GameStateManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one GameStateManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("GameStateManager initialized");
    }

    private void Start()
    {
        Debug.Log("GameStateManager Start method called");
        FindReferences();
        
        // Register this instance to static reference for easy access
        RegisterStaticInstance();
    }
    
    // Add a static reference that can be accessed without finding the object
    private static GameStateManager staticInstance;
    
    // Method to register the static instance
    private void RegisterStaticInstance()
    {
        staticInstance = this;
        Debug.Log("GameStateManager registered static instance");
    }
    
    // Static method to get the instance
    public static GameStateManager GetInstance()
    {
        if (staticInstance == null)
        {
            // Try to find it if not registered
            staticInstance = FindObjectOfType<GameStateManager>();
            
            // Create one if none exists
            if (staticInstance == null)
            {
                Debug.Log("Creating new GameStateManager through static accessor");
                GameObject go = new GameObject("GameStateManager");
                staticInstance = go.AddComponent<GameStateManager>();
            }
        }
        
        return staticInstance;
    }
    
    private void FindReferences()
    {
        // Find references if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log(player != null ? "Player found by tag" : "Player not found by tag!");
            
            // If tag fails, try to find by type
            if (player == null)
            {
                PlayerHealthController[] playerHealths = FindObjectsOfType<PlayerHealthController>();
                if (playerHealths.Length > 0)
                {
                    player = playerHealths[0].gameObject;
                    Debug.Log("Player found by PlayerHealthController component");
                }
            }
        }
            
        if (boss == null)
        {
            // Don't use tag directly since it might not be defined
            // Try finding by component type first
            BossHealthBarController[] bossControllers = FindObjectsOfType<BossHealthBarController>();
            if (bossControllers.Length > 0)
            {
                boss = bossControllers[0].gameObject;
                Debug.Log($"Boss found by BossHealthBarController component: {boss.name}");
            }
            else
            {
                // Also try BossCollider as a backup
                BossCollider[] bossColliders = FindObjectsOfType<BossCollider>();
                if (bossColliders.Length > 0)
                {
                    boss = bossColliders[0].gameObject;
                    Debug.Log($"Boss found by BossCollider component: {boss.name}");
                }
                else
                {
                    // Try name-based search as a last resort
                    GameObject bossObject = GameObject.Find("Boss");
                    if (bossObject != null)
                    {
                        boss = bossObject;
                        Debug.Log($"Boss found by name: {boss.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Could not find any boss object in the scene");
                    }
                }
            }
        }

        // Get health components
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealthController>();
            Debug.Log(playerHealth != null ? "PlayerHealthController found" : "PlayerHealthController not found!");
        }
            
        if (boss != null)
        {
            bossHealth = boss.GetComponent<BossHealthBarController>();
            Debug.Log(bossHealth != null ? "BossHealthBarController found" : "BossHealthBarController not found!");
        }
    }

    private void Update()
    {
        // Don't check if we're already transitioning
        if (isTransitioning)
            return;

        // Try to find references if they're missing
        if (player == null || boss == null || playerHealth == null || bossHealth == null)
        {
            FindReferences();
        }

        // Check win condition
        if (CheckWinCondition())
        {
            Debug.Log("Win condition met! Transitioning to Win scene.");
            StartCoroutine(TransitionToScene(winSceneIndex));
        }
        // Check loss condition
        else if (CheckLossCondition())
        {
            Debug.Log("Loss condition met! Transitioning to Loss scene.");
            StartCoroutine(TransitionToScene(loseSceneIndex));
        }
    }

    private bool CheckWinCondition()
    {
        // Boss object is destroyed/removed from scene
        if (boss == null)
        {
            Debug.Log("Win condition: Boss object is null");
            return true;
        }

        // Boss health component is missing or null
        if (bossHealth == null)
        {
            Debug.Log("Win condition: BossHealthBarController is null");
            return true;
        }

        // Boss health equals 0
        if (bossHealth.luongMauHienTai <= 0)
        {
            Debug.Log("Win condition: Boss health is 0 or less");
            return true;
        }

        return false;
    }

    private bool CheckLossCondition()
    {
        // Player object is destroyed/removed from scene
        if (player == null)
        {
            Debug.Log("Loss condition: Player object is null");
            return true;
        }

        // Player health component is missing or null
        if (playerHealth == null)
        {
            Debug.Log("Loss condition: PlayerHealthController is null");
            return true;
        }

        // Player health equals 0
        if (playerHealth.luongMauHienTai <= 0)
        {
            Debug.Log("Loss condition: Player health is 0 or less");
            return true;
        }

        return false;
    }

    private IEnumerator TransitionToScene(int sceneIndex)
    {
        isTransitioning = true;
        Debug.Log($"Starting transition to scene {sceneIndex} after {transitionDelay} seconds");

        // You can add fade effect or other transition effects here
        
        // Optional: Play transition sound
        // AudioSource.PlayClipAtPoint(transitionSound, Camera.main.transform.position);
        
        // Clean up resources if needed
        CleanupResources();
        
        yield return new WaitForSeconds(transitionDelay);
        
        Debug.Log($"Loading scene {sceneIndex} now");
        SceneManager.LoadScene(sceneIndex);
    }
    
    private void CleanupResources()
    {
        // Stop any ongoing processes, particle systems, etc.
        
        // Reset the singleton instance when changing scenes
        Instance = null;
        staticInstance = null;
        
        // Don't destroy this object on scene change - let it be destroyed naturally
        Destroy(gameObject);
    }
    
    // Public method to force scene transition (can be called from other scripts)
    public void ForceGameOver()
    {
        if (!isTransitioning)
        {
            Debug.Log("Force game over called");
            StartCoroutine(TransitionToScene(loseSceneIndex));
        }
    }
    
    // Public method to force win scene transition (can be called from other scripts)
    public void ForceWin()
    {
        if (!isTransitioning)
        {
            Debug.Log("Force win called");
            StartCoroutine(TransitionToScene(winSceneIndex));
        }
    }
    
    // Method that can be called from the BossHealthBarController when the boss is defeated
    public void NotifyBossDefeated()
    {
        Debug.Log("Boss defeat notification received by GameStateManager");
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(winSceneIndex));
        }
    }
}
