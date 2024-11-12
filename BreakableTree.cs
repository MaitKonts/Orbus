using UnityEngine;
using Unity.Netcode;

public class BreakableTree : NetworkBehaviour, IDamageable
{
    [SerializeField] private int health = 5; // Number of hits required to break the tree
    [SerializeField] private GameObject woodLogPrefab; // The "wood_log" prefab that drops upon breaking

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            BreakTree();
        }
    }

    private void BreakTree()
    {
        if (IsServer)
        {
            if (woodLogPrefab != null)
            {
                SpawnWoodLogServerRpc();
            }
            else
            {
                Debug.LogError("Wood log prefab is not assigned.");
            }
            Destroy(gameObject); // Destroy the tree on the server
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnWoodLogServerRpc()
    {
        var woodLog = Instantiate(woodLogPrefab, transform.position, Quaternion.identity);
        var networkObject = woodLog.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(); // Spawn for all clients
        }
        else
        {
            Debug.LogError("Wood log prefab does not have a NetworkObject component.");
        }
    }
}
