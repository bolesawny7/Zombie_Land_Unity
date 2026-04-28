using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Camera-relative WASD movement using a CharacterController.
    /// CharacterController gives us collision detection against walls
    /// without needing a Rigidbody, and lets us use OnControllerColliderHit
    /// for custom collision feedback.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5.5f;
        public float rotationSpeed = 12f;
        public float gravity = -20f;

        CharacterController controller;
        Camera mainCamera;
        Vector3 verticalVelocity;

        public bool IsMoving { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

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

            if (controller.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -1f;
            verticalVelocity.y += gravity * Time.deltaTime;

            Vector3 motion = inputDir * moveSpeed + verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            IsMoving = inputDir.sqrMagnitude > 0.01f;

            if (IsMoving)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
