using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Ensure the PauseMenu instance persists across scenes
    void Awake()
    {
        // Check if an instance already exists
        PauseMenu[] pauseMenus = FindObjectsOfType<PauseMenu>();
        if (pauseMenus.Length > 1)
        {
            // Destroy the duplicate instance
            Destroy(gameObject);
        }
        else
        {
            // Set this instance as the singleton
            DontDestroyOnLoad(gameObject);
            pauseMenu.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }


    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
