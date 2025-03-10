using UnityEngine;

public class BossHealthBarController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public BossHealthBar bossHealthBar;
    public float luongMauHienTai;
    public float luongMauToiDa = 12000;
    void Start()
    {
        luongMauHienTai = 6000;
        bossHealthBar.capNhatThanhMau(luongMauHienTai, luongMauToiDa);
    }

    // Update is called once per frame
    
}
