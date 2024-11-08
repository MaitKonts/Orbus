using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections;
namespace IvyMoon{
public class PlayerController : NetworkBehaviour, IDamageable
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float lookSpeed = 2f;
    public float groundCheckDistance = 0.1f;
    private InventoryDisplay inventoryDisplay;
    private HealthBar healthBar;
    private float currentSpeed;
    private float verticalVelocity;
    private Camera playerCamera;
    private float verticalLookRotation;
    private CharacterController characterController;
    private Animator animator;
    private bool isJumping = false;
    private bool isFalling = false;
    public int attackDamage = 1;
    public int maxHealth = 10;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<int> networkSpeed = new NetworkVariable<int>();
    private NetworkVariable<bool> networkIsJumping = new NetworkVariable<bool>();
    private NetworkVariable<int> selectedHotbarSlot = new NetworkVariable<int>(0); // Default to slot 0


    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inventoryDisplay = FindObjectOfType<InventoryDisplay>(); // Adjust this if you need a specific instance
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        healthBar = FindObjectOfType<HealthBar>();
        currentHealth.OnValueChanged += OnHealthChanged;
        if (IsLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            HandleMovement();
            HandleLook();
            HandleJump();

            if (Input.GetMouseButtonDown(0))
            {
                AttemptAttack();
            }

            // Send movement updates to the server
            UpdateMovementServerRpc(transform.position, transform.rotation, animator.GetInteger("Speed"));
        }
        else
        {
            // Sync position, rotation, and animation for non-local players
            transform.position = networkPosition.Value;
            transform.rotation = networkRotation.Value;
            animator.SetInteger("Speed", networkSpeed.Value);
            animator.SetBool("Jumping", networkIsJumping.Value); // Sync jump animation
        }
    }


    private void HandleMovement()
    {
        if (characterController == null || !characterController.gameObject.activeInHierarchy) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Determine the base speed (walk or run)
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Determine the animation speed based on direction
        if (vertical > 0)
        {
            animator.SetInteger("Speed", 2); // Forward
        }
        else if (vertical < 0)
        {
            animator.SetInteger("Speed", -2); // Backward
        }
        else if (horizontal > 0)
        {
            animator.SetInteger("Speed", 3); // Right
        }
        else if (horizontal < 0)
        {
            animator.SetInteger("Speed", -3); // Left
        }
        else
        {
            animator.SetInteger("Speed", 0); // Idle
        }

        characterController.Move(direction * currentSpeed * Time.deltaTime);
    }



    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
    }

    private void HandleJump()
    {
        bool isGrounded = characterController.isGrounded || IsGroundedRaycast();

        if (isGrounded)
        {
            isJumping = false;
            isFalling = false;
            verticalVelocity = 0;
            animator.SetBool("Falling", false);
            animator.SetBool("Jumping", false); // Reset jumping animation immediately

            if (Input.GetButtonDown("Jump"))
            {
                isJumping = true;
                verticalVelocity = jumpForce;
                animator.SetBool("Jumping", true);

                // Notify the server to update the jump state
                UpdateJumpStateServerRpc(true);
            }
            else
            {
                // Notify the server to reset the jump state
                UpdateJumpStateServerRpc(false);
            }
        }
        else
        {
            if (!isJumping)
            {
                isFalling = true;
                animator.SetBool("Falling", true);
            }
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 verticalMove = new Vector3(0, verticalVelocity, 0);
        characterController.Move(verticalMove * Time.deltaTime);
    }

    [ServerRpc]
    private void UpdateJumpStateServerRpc(bool isJumping)
    {
        networkIsJumping.Value = isJumping;
        UpdateJumpStateClientRpc(isJumping);
    }

    [ClientRpc]
    private void UpdateJumpStateClientRpc(bool isJumping)
    {
        animator.SetBool("Jumping", isJumping);
    }



    private bool IsGroundedRaycast()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(groundRay, groundCheckDistance);
    }

    [ServerRpc]
    private void UpdateMovementServerRpc(Vector3 position, Quaternion rotation, int speed)
    {
        networkPosition.Value = position;
        networkRotation.Value = rotation;
        networkSpeed.Value = speed;

        // Broadcast the updated position and rotation to all clients
        UpdateMovementClientRpc(position, rotation, speed);
    }

    [ClientRpc]
    private void UpdateMovementClientRpc(Vector3 position, Quaternion rotation, int speed)
    {
        if (!IsLocalPlayer)
        {
            transform.position = position;
            transform.rotation = rotation;
            animator.SetInteger("Speed", speed);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttemptAttackServerRpc(ulong targetNetworkObjectId, int damageAmount) {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetObject)) {
            if (targetObject != this.NetworkObject) { // Ensure we’re not targeting ourselves
                IDamageable damageable = targetObject.GetComponent<IDamageable>();
                if (damageable != null) {
                    // Apply damage on the server to the target player
                    damageable.TakeDamage(damageAmount);
                } else {
                    Debug.LogWarning("Target does not implement IDamageable.");
                }
            } else {
                Debug.LogWarning("Cannot damage self.");
            }
        } else {
            Debug.LogWarning("Target object not found on the server.");
        }
    }

    private void AttemptAttack() {
        InventoryItem selectedItem = GetSelectedHotbarItem();
        if (selectedItem == null) {
            Debug.LogWarning("No item selected in hotbar.");
            return;
        }

        // Perform a raycast locally to detect the target
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 4f)) {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            NetworkObject targetNetworkObject = hit.collider.GetComponent<NetworkObject>();

            // Allow attacking any IDamageable object, but prevent self-damage
            if (damageable != null && (targetNetworkObject == null || targetNetworkObject.NetworkObjectId != NetworkObject.NetworkObjectId)) {
                if (targetNetworkObject != null && targetNetworkObject.TryGetComponent(out PlayerController targetPlayer)) {
                    // If the target is another player, use server RPC to apply damage
                    TakeDamageServerRpc(targetNetworkObject.NetworkObjectId, selectedItem.damageValue, true);
                } else {
                    // Directly apply damage to non-player targets like pigs
                    damageable.TakeDamage(selectedItem.damageValue);
                }
            } else {
                Debug.LogWarning("Cannot attack self or object is not damageable.");
            }
        }
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc(int attackIndex)
    {
        string attackTrigger = "Attack" + attackIndex;
        animator.SetTrigger(attackTrigger);
        StartCoroutine(ResetAttackTrigger(attackTrigger, 0.5f));
    }

    private IEnumerator ResetAttackTrigger(string triggerName, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.ResetTrigger(triggerName);
    }

    private void UpdateHotbarSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectHotbarSlotServerRpc(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectHotbarSlotServerRpc(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectHotbarSlotServerRpc(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectHotbarSlotServerRpc(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectHotbarSlotServerRpc(4);
    }

    private InventoryItem GetSelectedHotbarItem() {
        int index = selectedHotbarSlot.Value;
        return inventoryDisplay.hotbarSlots[index].item; // Access hotbar slot from instance
    }


    [ServerRpc]
    private void SelectHotbarSlotServerRpc(int slotIndex) {
        selectedHotbarSlot.Value = slotIndex;
        UpdateHotbarSelectionClientRpc(slotIndex); // Sync selection across clients
    }

    [ClientRpc]
    private void UpdateHotbarSelectionClientRpc(int slotIndex) {
        selectedHotbarSlot.Value = slotIndex; // Update the slot for all clients
    }

    public void TakeDamage(int damage) {
        if (IsServer) {
            currentHealth.Value -= damage;
            if (currentHealth.Value <= 0) {
                HandleDeath();
            }

            // Targeted ClientRpc to update only the damaged player's UI
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };
            UpdateHealthUIForTargetClientRpc(currentHealth.Value, maxHealth, clientRpcParams);
        }
    }

    [ClientRpc]
    private void UpdateHealthUIForTargetClientRpc(int newHealth, int maxHealth, ClientRpcParams clientRpcParams = default) {
        if (IsOwner) { // Only the client who owns this object will update their health UI
            healthBar.UpdateHealthUI(newHealth, maxHealth);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(ulong targetNetworkObjectId, int damageAmount, bool isPlayerTarget) {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetObject)) {
            if (targetObject != this.NetworkObject) { // Ensure the target is not self
                if (isPlayerTarget && targetObject.TryGetComponent(out PlayerController targetPlayer)) {
                    targetPlayer.ApplyDamage(damageAmount);
                }
            }
        }
    }


    private void HandleDeath() {
        currentHealth.Value = 0;

        // Target the player who died to show the respawn screen
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        NotifyClientsOfDeathClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void NotifyClientsOfDeathClientRpc(ClientRpcParams clientRpcParams = default) {
        if (IsOwner) {
            FindObjectOfType<RespawnScreenManager>()?.ShowRespawnScreen();
        }
    }

    private void OnEnable() {
        if (IsOwner) {
            currentHealth.OnValueChanged += UpdateHealthUI;
        }
    }

    private void UpdateHealthUI(int previousValue, int newValue) {
        if (IsOwner && healthBar != null) { // Ensure health UI updates only for the local player
            healthBar.UpdateHealthUI(newValue, maxHealth);
        }
    }

    private void OnHealthChanged(int previousHealth, int newHealth)
    {
        if (IsOwner && healthBar != null) // Only update if it's the local player
        {
            healthBar.UpdateHealthUI(newHealth, maxHealth);
        }
    }


    public void RespawnPlayer() {
    currentHealth.Value = maxHealth; // Reset health
    // Set the player's position to a spawn point
    transform.position = GetSpawnPoint(); // Define GetSpawnPoint method to provide respawn location
    healthBar.UpdateHealthUI(currentHealth.Value, maxHealth); // Update health UI if applicable
    }

    private Vector3 GetSpawnPoint() {
        // Define your respawn logic, e.g., starting position or a random spawn point
        return new Vector3(0, 1, 0); // Example: a basic spawn point
    }

    public void ApplyDamage(int damageAmount) {
        if (IsServer) {
            currentHealth.Value -= damageAmount;
            if (currentHealth.Value <= 0) {
                HandleDeath();
            }

            // Targeted ClientRpc to update health UI only for the damaged player
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };
            UpdateHealthUIForTargetClientRpc(currentHealth.Value, maxHealth, clientRpcParams);
        }
    }


}
}
