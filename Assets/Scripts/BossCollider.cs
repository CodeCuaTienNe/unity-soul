using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Add this for coroutines


public class BossCollider : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;
     [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private float damageSoundVolume = 1.0f;
    private AudioSource audioSource;
    
    // Reference to the health bar script
    public BossHealthBar bossHealthBar;
    
    // Optional reference to a boss model (if it's a child of this object)
    public GameObject bossModel;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        currentHP = maxHP;
        
        // Initialize the health bar if assigned
        if (bossHealthBar != null)
        {
            Debug.Log("Initial health bar update: " + currentHP + "/" + maxHP);
            bossHealthBar.capNhatThanhMau(currentHP, maxHP);
        }
        else
        {
            Debug.LogError("No health bar assigned to BossCollider! Please assign it in the Inspector.");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        
        // Update the health bar
        if (bossHealthBar != null)
        {
            Debug.Log("Updating health bar after damage: " + currentHP + "/" + maxHP);
            bossHealthBar.capNhatThanhMau(currentHP, maxHP);
            PlayDamageSound();
        }
        else
        {
            Debug.LogError("Cannot update health bar: Reference is missing!");
        }

        Debug.Log($"Boss took {damage} damage. Current HP: {currentHP}/{maxHP}");

        // Check if boss is dead
        if (currentHP <= 0)
        {
            OnBossDeath();
        }
    }

    private void OnBossDeath()
    {
        // You can add any death effects here
        Debug.Log("Boss defeated!");
        
        // Start coroutine to destroy the boss after delay
        StartCoroutine(DestroyAfterDelay(3f));
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        // Wait for the specified number of seconds
        yield return new WaitForSeconds(delay);
        
        // Then destroy the gameObject
        Destroy(gameObject);
        SceneManager.LoadScene(4);
    }

    private void PlayDamageSound()
    {
        // Initialize audio source if needed
        if (audioSource == null)
        {
            // Try to get existing AudioSource
            audioSource = GetComponent<AudioSource>();
            
            // Create one if it doesn't exist
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Play sound if we have a clip
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound, damageSoundVolume);
        }
    }
}
