using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play3d()
    {
        SceneManager.LoadScene("3dGame");
    }

    public void Play2d()
    {
        SceneManager.LoadScene("2dGame");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
