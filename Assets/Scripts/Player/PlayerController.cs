using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Camera-relative WASD movement using a CharacterController.
    /// Hold <see cref="sprintKey"/> (Left Shift) to sprint, while
    /// `sprintStamina` &gt; 0. Stamina drains while sprinting and
    /// regenerates while not.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5.5f;
        public float sprintMultiplier = 1.7f;
        public float rotationSpeed = 12f;
        public float gravity = -20f;

        [Header("Sprint stamina")]
        public KeyCode sprintKey = KeyCode.LeftShift;
        public float maxStamina = 100f;
        public float sprintDrainPerSecond = 25f;
        public float staminaRegenPerSecond = 18f;
        public float staminaUnlockThreshold = 15f;

        public float Stamina { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsMoving { get; private set; }

        CharacterController controller;
        Camera mainCamera;
        PlayerStats stats;
        Vector3 verticalVelocity;
        bool sprintLocked;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            stats = GetComponent<PlayerStats>();
            Stamina = maxStamina;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            // Once the player is dead the GameManager has frozen time; this is
            // a belt-and-braces guard for the rare frame where time is still
            // ticking but Soul has just hit zero.
            if (stats != null && stats.IsDead) return;

            // Camera is built after the player by LevelBuilder, so we
            // resolve it lazily and cache the first non-null result.
            if (mainCamera == null) mainCamera = Camera.main;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camForward = Vector3.forward;
            Vector3 camRight = Vector3.right;
            if (mainCamera != null)
            {
                camForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
                camRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
            }

            Vector3 inputDir = camForward * v + camRight * h;
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
            IsMoving = inputDir.sqrMagnitude > 0.01f;

            UpdateSprint(IsMoving);

            float speed = walkSpeed * (IsSprinting ? sprintMultiplier : 1f);

            if (controller.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -1f;
            verticalVelocity.y += gravity * Time.deltaTime;

            Vector3 motion = inputDir * speed + verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            if (IsMoving)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        void UpdateSprint(bool moving)
        {
            bool wantSprint = Input.GetKey(sprintKey) && moving;

            // Once stamina runs out, lock sprint until the player has
            // regenerated past `staminaUnlockThreshold`. This avoids the
            // jittery "1-frame sprint" feeling near 0 stamina.
            if (Stamina <= 0f) sprintLocked = true;
            if (sprintLocked && Stamina >= staminaUnlockThreshold) sprintLocked = false;

            IsSprinting = wantSprint && !sprintLocked && Stamina > 0f;

            if (IsSprinting)
                Stamina = Mathf.Max(0f, Stamina - sprintDrainPerSecond * Time.deltaTime);
            else
                Stamina = Mathf.Min(maxStamina, Stamina + staminaRegenPerSecond * Time.deltaTime);
        }
    }
}
