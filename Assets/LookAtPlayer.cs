using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public GameObject player;
    private void Start()
    {
    
    }

    private void Update()
    {
        transform.LookAt(player.transform);
    }

}
