using UnityEngine;

namespace ZombieLand.Utility
{
    /// <summary>
    /// A lightweight third-person camera. Smoothly follows a target,
    /// supports a transient screen-shake effect (used when a zombie disturbs
    /// a memory) without fighting with its own positional smoothing.
    /// </summary>
    public class SmoothFollowCamera : MonoBehaviour
    {
        public static SmoothFollowCamera Instance;

        public Transform target;
        public Vector3 offset = new Vector3(0f, 14f, -10f);
        public float smoothTime = 0.15f;
        public Vector3 lookOffset = new Vector3(0f, 1f, 0f);

        Vector3 followVelocity;
        float shakeTimeRemaining;
        float shakeMagnitude;
        Vector3 shakeOffset;

        void Awake() { Instance = this; }

        void LateUpdate()
        {
            if (!target) return;

            Vector3 desired = target.position + offset;
            Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, smoothTime);

            if (shakeTimeRemaining > 0f)
            {
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * shakeMagnitude;
                shakeTimeRemaining -= Time.deltaTime;
                if (shakeTimeRemaining <= 0f) shakeOffset = Vector3.zero;
            }

            transform.position = smoothPosition + shakeOffset;
            transform.LookAt(target.position + lookOffset);
        }

        public void Shake(float duration, float magnitude)
        {
            // Always take the longer/stronger shake when stacking.
            shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
            shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);
        }
    }
}
