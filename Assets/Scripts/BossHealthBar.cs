using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Image _healBar;

    public void capNhatThanhMau(float luongMauHienTai, float luongMauToiDa)
    {
        if (_healBar == null)
        {
            Debug.LogError("Health bar Image reference is missing!");
            return;
        }

        float ratioValue = luongMauHienTai / luongMauToiDa;
        Debug.Log($"Updating health bar: {luongMauHienTai}/{luongMauToiDa} = {ratioValue}");
        _healBar.fillAmount = ratioValue;
    }

    // For testing in the editor
    [ContextMenu("Test Health Bar 50%")]
    public void TestHealthBar()
    {
        capNhatThanhMau(50, 100);
    }

    [ContextMenu("Test Health Bar 100%")]
    public void TestHealthBarFull()
    {
        capNhatThanhMau(100, 100);
    }
}
