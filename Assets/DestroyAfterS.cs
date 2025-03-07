using UnityEngine;

public class DestroyAfterS : MonoBehaviour
{
    [Tooltip("Time in seconds before this object is destroyed")]
    public float lifetime = 5f;

    [Tooltip("Whether to show debug message when destroyed")]
    public bool showDebugMessage = false;

    // Start is called before the first frame update
    void Start()
    {
        // Schedule destruction after the specified lifetime
        Destroy(gameObject, lifetime);

        if (showDebugMessage)
        {
            Debug.Log(gameObject.name + " will be destroyed after " + lifetime + " seconds");
        }
    }
}
