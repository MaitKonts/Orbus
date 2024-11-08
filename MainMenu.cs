using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI; // Required for UI elements

public class MainMenu : MonoBehaviour
{
    public Button singleplayerButton;
    public Button multiplayerButton;
    public Button optionsButton;
    public Button quitButton;

    void Start()
    {
        // Attach button click events to methods
        singleplayerButton.onClick.AddListener(StartSingleplayer);
        multiplayerButton.onClick.AddListener(StartMultiplayer);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void StartSingleplayer()
    {
        // Load the singleplayer scene
        SceneManager.LoadScene("Singleplayer"); // Ensure you have this scene created
    }

    public void StartMultiplayer()
    {
        // Load the multiplayer scene
        SceneManager.LoadScene("Multiplayer"); // Ensure you have this scene created
    }

    public void OpenOptions()
    {
        // Load the options scene or open options menu
        SceneManager.LoadScene("Options"); // Ensure you have this scene created
    }

    public void QuitGame()
    {
        // Quit the game
        Application.Quit();
        Debug.Log("Game is quitting...");
    }
}
