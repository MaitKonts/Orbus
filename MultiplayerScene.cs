using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI; // Required for UI elements
using Unity.Netcode; // Required for networking

public class MultiplayerScene : MonoBehaviour
{
    public InputField ipAddressInputField; // Reference to the input field for the IP address
    public Button hostServerButton; // Reference to the button for hosting a server
    public Button connectButton; // Reference to the button for connecting to a server
    public Button backButton; // Reference to the back button

    void Start()
    {
        // Attach button click events to methods
        hostServerButton.onClick.AddListener(HostServer);
        connectButton.onClick.AddListener(ConnectToServer);
        backButton.onClick.AddListener(GoBackToMainMenu);
    }

    // Method to host the server
    public void HostServer()
    {
        SceneManager.LoadScene("MainGame"); // Ensure this matches your actual game scene name
        if (!NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Server started");
        }

    }

    // Method to connect to the server
    public void ConnectToServer()
    {
        string ipAddress = ipAddressInputField.text;
        if (!string.IsNullOrEmpty(ipAddress))
        {
            NetworkManager.Singleton.GetComponent<NetworkManager>().StartClient();
            Debug.Log($"Connecting to server at {ipAddress}...");
        }
        else
        {
            Debug.LogWarning("Please enter a valid IP address.");
        }
    }

    // Method to go back to the main menu
    public void GoBackToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu"); // Ensure you have a scene named "MainMenu"
    }
    
}
