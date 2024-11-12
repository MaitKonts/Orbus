using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Vendor : NetworkBehaviour
{
    public List<GameObject> players; // List of players to check distance
    private GameObject[] playersToAdd;
    private GameObject currentInteractingPlayer = null;

    public float interactionRange; // Set the size of the interaction trigger
    public bool showInteractionRange;
    private ActivateUI myUI;

    private NetworkVariable<bool> isInteracting = new NetworkVariable<bool>(false);

    private void Awake()
    {
        myUI = GetComponentInChildren<ActivateUI>();
        myUI.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playersToAdd = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in playersToAdd)
            {
                players.Add(player);
            }
        }

        isInteracting.OnValueChanged += OnInteractionStateChanged;
    }

    private void Update()
    {
        if (IsClient && !IsOwner)
        {
            CheckForInteractionInput();
        }

        if (IsServer)
        {
            CheckForInteractionRange();
        }
    }

    private void CheckForInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Assuming 'E' is the interaction key
        {
            float distance = Vector3.Distance(transform.position, NetworkManager.Singleton.LocalClient.PlayerObject.transform.position);
            if (distance < interactionRange)
            {
                RequestInteractionServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractionServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < interactionRange && !isInteracting.Value)
        {
            isInteracting.Value = true;
            currentInteractingPlayer = player;
        }
    }

    private void CheckForInteractionRange()
    {
        if (currentInteractingPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, currentInteractingPlayer.transform.position);
            if (distance >= interactionRange)
            {
                isInteracting.Value = false;
                currentInteractingPlayer = null;
            }
        }
    }

    private void OnInteractionStateChanged(bool previousValue, bool newValue)
    {
        myUI.enabled = newValue;
        myUI.uIToToggle.SetActive(newValue);
    }

    private void OnDrawGizmos()
    {
        if (showInteractionRange)
        {
            if (interactionRange <= 0) return;
            Gizmos.color = new Color(0, 1, 0, 0.1f); // Green
            Gizmos.DrawSphere(transform.position, interactionRange);
        }
    }
}
