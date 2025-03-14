using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Skip : MonoBehaviour
{
    [Tooltip("Time in seconds before automatically skipping to the next scene")]
    public float autoSkipTime = 114f;
    
    [Tooltip("Whether to show debug messages")]
    public bool showDebug = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start the coroutine to wait and then skip
        StartCoroutine(WaitAndSkip());
    }
    
    // Coroutine to wait for the specified time and then load the next scene
    private IEnumerator WaitAndSkip()
    {
        if (showDebug) Debug.Log($"Will automatically skip to next scene after {autoSkipTime} seconds");
        
        // Wait for the specified time
        yield return new WaitForSeconds(autoSkipTime);
        
        // Load the next scene
        if (showDebug) Debug.Log("Auto-skip time reached, loading next scene");
        LoadNextScene();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Manual skip with Tab key
        if (Input.anyKeyDown)
        {
            if (showDebug) Debug.Log("Tab key pressed, skipping to next scene");
            LoadNextScene();
        }
    }
    
    // Method to load the next scene
    private void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available to load!");
        }
    }
}
