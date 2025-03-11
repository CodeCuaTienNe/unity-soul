using UnityEngine;

public class SwordCollider : MonoBehaviour
{
    public int damage = 10;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the boss
        if (collision.gameObject.CompareTag("Boss"))
        {
            // Get the BossCollider component from the boss object
            BossCollider bossCollider = collision.gameObject.GetComponent<BossCollider>();
            if (bossCollider != null)
            {
                // Call the TakeDamage method on the boss
                bossCollider.TakeDamage(damage);
            }
        }
    }
}