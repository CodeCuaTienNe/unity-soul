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
