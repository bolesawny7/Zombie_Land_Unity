using UnityEngine;

namespace ZombieLand.Utility
{
    /// <summary>
    /// Third-person orbit camera. The player moves the mouse to orbit
    /// horizontally and tilt vertically around the target. Smooth follow,
    /// configurable distance, and screen-shake support are preserved.
    /// </summary>
    public class SmoothFollowCamera : MonoBehaviour
    {
        public static SmoothFollowCamera Instance;

        public Transform target;
        public float distance = 12f;
        public float heightOffset = 5f;
        public float smoothTime = 0.12f;
        public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Mouse orbit")]
        public float mouseSensitivityX = 3f;
        public float mouseSensitivityY = 1.5f;
        public float minPitch = 10f;
        public float maxPitch = 80f;

        float yaw;
        float pitch = 35f;
        Vector3 followVelocity;

        float shakeTimeRemaining;
        float shakeMagnitude;
        Vector3 shakeOffset;

        void Awake()
        {
            Instance = this;
            yaw = 0f;
            pitch = 35f;
        }

        void LateUpdate()
        {
            if (!target) return;

            // Only orbit when the game is actually running (timeScale > 0
            // or at least the cursor is locked).
            if (Time.timeScale > 0f)
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivityX;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredOffset = rotation * new Vector3(0f, 0f, -distance);
            desiredOffset.y += heightOffset;

            Vector3 desiredPosition = target.position + desiredOffset;
            Vector3 smoothPosition = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref followVelocity, smoothTime);

            // Shake.
            if (shakeTimeRemaining > 0f)
            {
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f) * shakeMagnitude;
                shakeTimeRemaining -= Time.unscaledDeltaTime;
                if (shakeTimeRemaining <= 0f) shakeOffset = Vector3.zero;
            }

            transform.position = smoothPosition + shakeOffset;
            transform.LookAt(target.position + lookOffset);
        }

        public void Shake(float duration, float magnitude)
        {
            shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
        }
    }
}
