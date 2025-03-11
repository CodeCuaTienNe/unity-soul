using UnityEngine;

public class BossCollider : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;
    
    // Reference to the health bar script
    public BossHealthBar bossHealthBar;

    void Start()
    {
        currentHP = maxHP;
        // Initialize the health bar
        bossHealthBar.capNhatThanhMau(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        
        // Update the health bar
        bossHealthBar.capNhatThanhMau(currentHP, maxHP);

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
        
        // Optional: Destroy the boss object
        Destroy(gameObject);
    }
}