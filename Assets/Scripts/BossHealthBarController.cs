using UnityEngine;

public class BossHealthBarController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public BossHealthBar bossHealthBar;
    public float luongMauHienTai;
    public float luongMauToiDa = 12000;
    
    void Start()
    {
        luongMauHienTai = luongMauToiDa; // Start with full health
        bossHealthBar.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
    }

    // Method to handle taking damage
    public void TakeDamage(float damage)
    {
        luongMauHienTai -= damage;
        
        // Ensure health doesn't go below zero
        if (luongMauHienTai < 0)
        {
            luongMauHienTai = 0;
        }
        
        // Update the boss health bar
        bossHealthBar.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
        
        // Check if boss is defeated
        if (luongMauHienTai <= 0)
        {
            Debug.Log("Boss defeated!");
            // You can add boss defeat logic here
        }
    }
}