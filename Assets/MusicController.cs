using UnityEngine;

public class MusicController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load the music file
        if (backgroundMusic == null)
        {
            backgroundMusic = Resources.Load<AudioClip>("Music/theme1");
        }
        
        // Configure and play the audio
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Background music clip not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Any runtime adjustments to music can go here
    }
}
