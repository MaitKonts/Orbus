using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI; // Required for UI elements

public class SinglePlayerScene : MonoBehaviour
{
    public Button startGameButton; // Reference to the start game button
    public Button backButton; // Reference to the back button

    void Start()
    {
        Debug.Log("SinglePlayerScene script loaded."); // Log to confirm the script is running
        startGameButton.onClick.AddListener(StartSinglePlayerGame);
        backButton.onClick.AddListener(GoBackToMainMenu);
    }

    public void StartSinglePlayerGame()
    {
        Debug.Log("Starting single-player game...");
        SceneManager.LoadScene("MainGame");
    }

    public void GoBackToMainMenu()
    {
        Debug.Log("Going back to main menu...");
        SceneManager.LoadScene("MainMenu");
    }

}
