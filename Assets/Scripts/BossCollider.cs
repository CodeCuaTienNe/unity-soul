using UnityEngine;
using System.Collections; // Add this for coroutines

public class BossCollider : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;
    
    // Reference to the health bar script
    public BossHealthBar bossHealthBar;
    
    // Optional reference to a boss model (if it's a child of this object)
    public GameObject bossModel;

    void Start()
    {
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
    }
}