using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TreasureChest : NetworkBehaviour
{
    public List<GameObject> players; // List of players to check distance
    private GameObject[] playersToAdd;
    private GameObject playerToCheck;

    public float openRange; // Set the size of the open trigger
    public bool showOpenRange;
    private ActivateUI myUI;

    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);

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

        isOpen.OnValueChanged += OnOpenStateChanged;
    }

    private void Update()
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            playerToCheck = players[i]; // Reference the current object in the loop
            if (Vector3.Distance(transform.position, playerToCheck.transform.position) < openRange)
            {
                if (!isOpen.Value)
                {
                    isOpen.Value = true;
                }
            }
            else
            {
                if (isOpen.Value)
                {
                    isOpen.Value = false;
                }
            }
        }
    }

    private void OnOpenStateChanged(bool previousValue, bool newValue)
    {
        myUI.enabled = newValue;
        myUI.uIToToggle.SetActive(newValue);
    }

    private void OnDrawGizmos()
    {
        if (showOpenRange)
        {
            if (openRange <= 0) return;
            Gizmos.color = new Color(1, 0, 0, 0.1f); // Red
            Gizmos.DrawSphere(transform.position, openRange);
        }
    }
}
