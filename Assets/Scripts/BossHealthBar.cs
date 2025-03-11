using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Image _healBar;

    public void capNhatThanhMau(float luongMauHienTai, float luongMauToiDa)
    {
        _healBar.fillAmount = luongMauHienTai / luongMauToiDa;
    }
}
