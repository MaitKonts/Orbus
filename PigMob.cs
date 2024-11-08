using UnityEngine;
using Unity.Netcode;

public class PigController : NetworkBehaviour, IDamageable
{
    // Health properties
    public int maxHealth = 5;
    private NetworkVariable<int> health = new NetworkVariable<int>(5);
    private bool isDead = false;
    public GameObject itemPickupPrefab;

    // Movement properties
    public float moveSpeed = 2f;
    public float changeDirectionTime = 2f;
    public float wanderRadius = 5f;

    private Vector3 targetPosition;
    private float changeDirectionTimer;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        SetNewTargetPosition();
    }

    private void Update()
    {
        if (!IsServer || isDead) return;

        MoveTowardsTarget();

        changeDirectionTimer += Time.deltaTime;
        if (changeDirectionTimer >= changeDirectionTime)
        {
            SetNewTargetPosition();
            changeDirectionTimer = 0f;
        }
    }

    private void MoveTowardsTarget()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        animator.SetBool("isWalking", true);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }

    private void SetNewTargetPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        targetPosition = new Vector3(randomDirection.x, transform.position.y, randomDirection.z);
    }

    public void TakeDamage(int damage)
    {
        if (IsClient)
        {
            Debug.Log("Client is requesting to deal damage: " + damage);
            TakeDamageServerRpc(damage);
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(int damage)
    {
        if (isDead) return;

        Debug.Log("Server is processing damage: " + damage);
        health.Value -= damage;

        if (health.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("isDead");
        animator.SetBool("isWalking", false);
        Invoke(nameof(Despawn), 2f);
        DropItem();
        NotifyClientsOfDeathClientRpc();
    }

    [ClientRpc]
    private void NotifyClientsOfDeathClientRpc()
    {
        if (!IsServer)
        {
            animator.SetTrigger("isDead");
            animator.SetBool("isWalking", false);
        }
    }

    private void Despawn()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    private void DropItem()
    {
        if (itemPickupPrefab != null)
        {
            GameObject instantiatedObject = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
            NetworkObject networkObject = instantiatedObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
        }
    }
}
