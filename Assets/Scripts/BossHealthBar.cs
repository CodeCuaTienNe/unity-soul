using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image _healBar;

    public void capNhatThanhMau(float luongMauHienTai, float luongMauToiDa)
    {
        _healBar.fillAmount = luongMauHienTai / luongMauToiDa;
    }
}
