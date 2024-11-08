using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace IvyMoon{
public class RespawnScreenManager : MonoBehaviour { // Ensure MonoBehaviour is spelled correctly
    public Button respawnButton;
    public Button quitButton;
    public GameObject respawnScreen;

    private void Start() {
        respawnScreen.SetActive(false);
        respawnButton.onClick.AddListener(OnRespawnClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void ShowRespawnScreen() {
        respawnScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRespawnClicked() {
        Time.timeScale = 1f;
        respawnScreen.SetActive(false);
        FindObjectOfType<PlayerController>()?.RespawnPlayer();
    }

    private void OnQuitClicked() {
        SceneManager.LoadScene("MainMenu");
    }
}
}
