using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections;

namespace IvyMoon
{
    public class PlayerController : NetworkBehaviour, IDamageable
    {
        [SerializeField] private Transform handBone;

        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpForce = 8f;
        private float defaultGravity = -20f;
        public float gravity;
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
        private GameObject currentHeldItem;
        [SerializeField] private InventoryItemList itemList;
        public float swimSpeed = 3f; // Speed while swimming
        public float ascendSpeed = 2f; // Speed for ascending while in water
        public float waterGravity = -1f; // Reduced gravity in water
        private bool isInWater = false;

        private bool isJumping = false;
        private bool isFalling = false;
        public int attackDamage = 1;
        public int maxHealth = 10;
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>(10);
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<int> networkSpeed = new NetworkVariable<int>();
        private NetworkVariable<bool> networkIsJumping = new NetworkVariable<bool>();
        private NetworkVariable<int> selectedHotbarSlot = new NetworkVariable<int>(0); // Default to slot 0
        public NetworkVariable<FixedString128Bytes> networkSelectedItem = new NetworkVariable<FixedString128Bytes>();

        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            inventoryDisplay = FindObjectOfType<InventoryDisplay>();
            healthBar = FindObjectOfType<HealthBar>();

            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }

            currentHealth.OnValueChanged += OnHealthChanged;
            gravity = defaultGravity;

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
                HandleLook();
                if (isInWater)
                {
                    HandleSwimming();
                }
                else
                {
                    HandleMovement();
                    HandleJump();
                }
                if (Input.GetMouseButtonDown(0))
                {
                    AttemptAttack();
                }

                // Send movement updates to the server only if there's a significant change
                if (Vector3.Distance(transform.position, networkPosition.Value) > 0.01f ||
                    Quaternion.Angle(transform.rotation, networkRotation.Value) > 0.1f)
                {
                    UpdateMovementServerRpc(transform.position, transform.rotation, animator.GetInteger("Speed"));
                }
            }
            else
            {
                transform.position = networkPosition.Value;
                transform.rotation = networkRotation.Value;
                animator.SetInteger("Speed", networkSpeed.Value);
                animator.SetBool("Jumping", networkIsJumping.Value);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("WaterZone"))
            {
                isInWater = true;
                EnterWater();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("WaterZone"))
            {
                isInWater = false;
                ExitWater();
            }
        }

        private void EnterWater()
        {
            currentSpeed = swimSpeed;
            gravity = waterGravity;
            characterController.stepOffset = 0;
        }

        private void ExitWater()
        {
            currentSpeed = walkSpeed;
            gravity = defaultGravity;
            characterController.stepOffset = 0.3f;
        }

        private void HandleMovement()
        {
            if (characterController == null || !characterController.gameObject.activeInHierarchy) return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 direction = (transform.forward * vertical + transform.right * horizontal).normalized;

            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            int speed = 0;
            if (vertical > 0) speed = 2;
            else if (vertical < 0) speed = -2;
            else if (horizontal > 0) speed = 3;
            else if (horizontal < 0) speed = -3;

            if (animator.GetInteger("Speed") != speed)
            {
                animator.SetInteger("Speed", speed);
            }

            characterController.Move(direction * currentSpeed * Time.deltaTime);
        }

        private void HandleSwimming()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 direction = (transform.forward * vertical + transform.right * horizontal).normalized;
            characterController.Move(direction * swimSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.Space))
            {
                characterController.Move(Vector3.up * ascendSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                characterController.Move(Vector3.down * ascendSpeed * Time.deltaTime);
            }

            Vector3 buoyancy = Vector3.up * Mathf.Abs(gravity) * Time.deltaTime;
            characterController.Move(buoyancy);
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
                animator.SetBool("Jumping", false);

                if (Input.GetButtonDown("Jump"))
                {
                    isJumping = true;
                    verticalVelocity = jumpForce;
                    animator.SetBool("Jumping", true);
                    UpdateJumpStateServerRpc(true);
                }
                else
                {
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
        private void AttemptAttackServerRpc(ulong targetNetworkObjectId, int damageAmount)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetObject))
            {
                if (targetObject != this.NetworkObject)
                {
                    IDamageable damageable = targetObject.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damageAmount);
                    }
                    else
                    {
                        Debug.LogWarning("Target does not implement IDamageable.");
                    }
                }
                else
                {
                    Debug.LogWarning("Cannot damage self.");
                }
            }
            else
            {
                Debug.LogWarning("Target object not found on the server.");
            }
        }

        private void AttemptAttack()
        {
            InventoryItem selectedItem = GetSelectedHotbarItem();
            if (selectedItem == null)
            {
                Debug.LogWarning("No item selected in hotbar.");
                return;
            }

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 4f))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                NetworkObject targetNetworkObject = hit.collider.GetComponent<NetworkObject>();

                if (damageable != null && targetNetworkObject != null && targetNetworkObject.NetworkObjectId != NetworkObject.NetworkObjectId)
                {
                    if (damageable is BreakableTree)
                    {
                        if (selectedItem.itemName == "Axe")
                        {
                            damageable.TakeDamage(selectedItem.damageValue);
                        }
                        else
                        {
                            Debug.LogWarning("An Axe is required to break the tree.");
                        }
                    }
                    else
                    {
                        damageable.TakeDamage(selectedItem.damageValue);
                    }
                }
                else
                {
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

        private InventoryItem GetSelectedHotbarItem()
        {
            int index = selectedHotbarSlot.Value;
            return inventoryDisplay.hotbarSlots[index].item;
        }

        [ServerRpc]
        private void SelectHotbarSlotServerRpc(int slotIndex)
        {
            selectedHotbarSlot.Value = slotIndex;
            UpdateHotbarSelectionClientRpc(slotIndex);
        }

        [ClientRpc]
        private void UpdateHotbarSelectionClientRpc(int slotIndex)
        {
            selectedHotbarSlot.Value = slotIndex;
        }

        public void TakeDamage(int damage)
        {
            if (IsServer)
            {
                currentHealth.Value -= damage;
                if (currentHealth.Value <= 0)
                {
                    HandleDeath();
                }

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
        private void UpdateHealthUIForTargetClientRpc(int newHealth, int maxHealth, ClientRpcParams clientRpcParams = default)
        {
            if (IsOwner)
            {
                healthBar.UpdateHealthUI(newHealth, maxHealth);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(ulong targetNetworkObjectId, int damageAmount, bool isPlayerTarget)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var targetObject))
            {
                if (targetObject != this.NetworkObject)
                {
                    if (isPlayerTarget && targetObject.TryGetComponent(out PlayerController targetPlayer))
                    {
                        targetPlayer.ApplyDamage(damageAmount);
                    }
                }
            }
        }

        private void HandleDeath()
        {
            currentHealth.Value = 0;

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
        private void NotifyClientsOfDeathClientRpc(ClientRpcParams clientRpcParams = default)
        {
            if (IsOwner)
            {
                FindObjectOfType<RespawnScreenManager>()?.ShowRespawnScreen();
            }
        }

        private void OnEnable()
        {
            if (IsOwner)
            {
                currentHealth.OnValueChanged += UpdateHealthUI;
            }
        }

        private void UpdateHealthUI(int previousValue, int newValue)
        {
            if (IsOwner && healthBar != null)
            {
                healthBar.UpdateHealthUI(newValue, maxHealth);
            }
        }

        private void OnHealthChanged(int previousHealth, int newHealth)
        {
            if (IsOwner && healthBar != null)
            {
                healthBar.UpdateHealthUI(newHealth, maxHealth);
            }
        }

        public void RespawnPlayer()
        {
            currentHealth.Value = maxHealth;
            transform.position = GetSpawnPoint();
            healthBar.UpdateHealthUI(currentHealth.Value, maxHealth);
        }

        private Vector3 GetSpawnPoint()
        {
            return new Vector3(0, 1, 0);
        }

        public void ApplyDamage(int damageAmount)
        {
            if (IsServer)
            {
                currentHealth.Value -= damageAmount;
                if (currentHealth.Value <= 0)
                {
                    HandleDeath();
                }

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

        public void SetSelectedHotbarItem(string itemName)
        {
            if (IsOwner)
            {
                UpdateSelectedItemServerRpc(itemName);

                if (IsHost)
                {
                    Debug.Log($"Host selected item: {itemName}");
                    DisplaySelectedItemModel(itemName);
                }
            }
        }

        [ClientRpc]
        public void ForceItemDisplayClientRpc(string itemName)
        {
            DisplaySelectedItemModel(itemName);
        }

        [ServerRpc]
        private void UpdateSelectedItemServerRpc(string itemName)
        {
            networkSelectedItem.Value = new FixedString128Bytes(itemName);
            Debug.Log($"Server received selected item: {itemName}");

            UpdateSelectedItemClientRpc(itemName);
        }

        [ClientRpc]
        private void UpdateSelectedItemClientRpc(string itemName)
        {
            if (!IsHost)
            {
                Debug.Log($"Client received item update: {itemName}");
                DisplaySelectedItemModel(itemName);
            }
        }

        public void DisplaySelectedItemModel(string itemName)
        {
            Debug.Log($"Attempting to display item '{itemName}' for player");

            if (itemList == null)
            {
                Debug.LogError("InventoryItemList is not loaded.");
                return;
            }

            if (currentHeldItem != null)
            {
                Destroy(currentHeldItem);
            }

            InventoryItem selectedItem = itemList.GetItem(itemName);
            if (selectedItem == null)
            {
                Debug.LogWarning($"Item '{itemName}' not found in InventoryItemList.");
                return;
            }

            if (selectedItem.itemObject != null)
            {
                currentHeldItem = Instantiate(selectedItem.itemObject, handBone);
                currentHeldItem.transform.localPosition = selectedItem.handPositionOffset;
                currentHeldItem.transform.localRotation = Quaternion.Euler(selectedItem.handRotationOffset);
                Debug.Log($"Displayed item '{itemName}' in player's hand.");
            }
            else
            {
                Debug.LogWarning($"Item '{itemName}' has no associated itemObject.");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            networkSelectedItem.OnValueChanged += (oldValue, newValue) =>
            {
                Debug.Log($"OnValueChanged triggered: Old = {oldValue}, New = {newValue}");
                DisplaySelectedItemModel(newValue.ToString());
            };

            if (IsOwner && IsHost && !string.IsNullOrEmpty(networkSelectedItem.Value.ToString()))
            {
                Debug.Log($"Host initial display: {networkSelectedItem.Value.ToString()}");
                DisplaySelectedItemModel(networkSelectedItem.Value.ToString());
            }
        }
    }
}
