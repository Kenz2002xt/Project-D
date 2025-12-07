using UnityEngine;
using UnityEngine.SceneManagement;

//system to load in scenes 
//used to attach to buttons throughout gameplay
//Taken from project B

public class SceneLoader : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenControls()
    {
        SceneManager.LoadScene("ControlsScene");
    }
    public void OpenInstructions()
    {
        SceneManager.LoadScene("InstructScene");
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene("CreditScene");
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OpenWin()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void OpenGameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}