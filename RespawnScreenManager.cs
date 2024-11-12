using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace IvyMoon
{
    public class RespawnScreenManager : MonoBehaviour
    {
        public Button respawnButton;
        public Button quitButton;
        public GameObject respawnScreen;

        private void Start()
        {
            if (respawnScreen == null || respawnButton == null || quitButton == null)
            {
                Debug.LogError("RespawnScreenManager: One or more UI components are not assigned.");
                return;
            }

            respawnScreen.SetActive(false);
            respawnButton.onClick.AddListener(OnRespawnClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        public void ShowRespawnScreen()
        {
            if (respawnScreen != null)
            {
                respawnScreen.SetActive(true);
                Time.timeScale = 0f;
                Debug.Log("Respawn screen shown, game paused.");
            }
        }

        private void OnRespawnClicked()
        {
            Time.timeScale = 1f;
            if (respawnScreen != null)
            {
                respawnScreen.SetActive(false);
            }

            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.RespawnPlayer();
                Debug.Log("Player respawned.");
            }
            else
            {
                Debug.LogWarning("No PlayerController found in the scene.");
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quitting to MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
