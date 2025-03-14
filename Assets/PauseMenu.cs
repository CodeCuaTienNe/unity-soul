using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;

    void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("PauseMenu: pauseMenuUI is not assigned!");
            return;
        }
        pauseMenuUI.SetActive(false);
        SetCursorState(false);
        Debug.Log("PauseMenu initialized");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC key pressed");
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Resume()
    {
        Debug.Log("Resuming game");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        SetCursorState(false);
    }

    void Pause()
    {
        Debug.Log("Pausing game");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        SetCursorState(true);
    }

    public void LoadMainMenu()
    {
        Debug.Log("Loading main menu");
        Time.timeScale = 1f;
        SetCursorState(true);
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        gameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2 );
    }
}
