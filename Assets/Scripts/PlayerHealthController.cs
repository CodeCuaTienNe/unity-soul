using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public HealthBarScript healthBarScript;
    public float luongMauHienTai;
    public float luongMauToiDa = 10;
    public float tocDoGiamMau = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        luongMauHienTai = luongMauToiDa;
        healthBarScript.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
    }

    private void OnMouseDown()
    {
        luongMauHienTai = luongMauHienTai - 1;
        healthBarScript.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
    }

    // New method to take damage from rocks
    public void TakeDamage(float damage)
    {
        luongMauHienTai -= damage;
        
        // Make sure health doesn't go below zero
        if (luongMauHienTai < 0)
        {
            luongMauHienTai = 0;
        }
        
        // Update the health bar
        healthBarScript.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
        
        // Check if player died
        if (luongMauHienTai <= 0)
        {
            Debug.Log("Player died!");
            // You can add additional death logic here
        }
    }

    //void Update()
    //{
    //    // Giảm máu theo thời gian
    //    luongMauHienTai -= tocDoGiamMau * Time.deltaTime;

    //    // Đảm bảo lượng máu không nhỏ hơn 0
    //    if (luongMauHienTai < 0)
    //    {
    //        luongMauHienTai = 0;
    //        Destroy(this.gameObject);
    //    }

    //    // Cập nhật thanh máu
    //    healthBarScript.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
    //}
}