using UnityEngine;

public class Checkkk : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnCollisionEnter(Collision collision)
	{
		// In ra tên của đối tượng mà đối tượng này va chạm
		Debug.Log("Va chạm bắt đầu với: aaaaaaaaaaaaaaaaaaaaaaaaaaa");
		Destroy(collision.gameObject);
	}

	private void OnCollisionStay(Collision collision)
	{
		// In ra tên của đối tượng mà đối tượng này đang va chạm
		Debug.Log("Đang va chạm với: aaaaaaaaaaaaaaaaa");
		Destroy(collision.gameObject);
	}

	private void OnCollisionExit(Collision collision)
	{
		// In ra tên của đối tượng mà đối tượng này đã kết thúc va chạm
		Debug.Log($"Kết thúc va chạm với: {collision.gameObject.name}");
	}
}
