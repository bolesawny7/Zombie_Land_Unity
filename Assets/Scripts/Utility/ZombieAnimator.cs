using UnityEngine;

namespace ZombieLand.Utility
{
    /// <summary>
    /// Procedural shamble for a primitive zombie. Body sways side-to-side,
    /// head bobs, and arms swing — gently while wandering, harder while
    /// chasing. No Animator/Animation Controller required.
    /// </summary>
    public class ZombieAnimator : MonoBehaviour
    {
        public Transform body;
        public Transform head;
        public Transform leftArm;
        public Transform rightArm;

        public float swaySpeed = 4f;
        public float swayAmount = 6f;     // degrees
        public float bobSpeed = 6f;
        public float bobAmount = 0.06f;   // metres
        public float armSwing = 25f;      // degrees

        Vector3 lastWorldPos;
        Vector3 bodyRestEuler;
        Vector3 headRestLocal;
        Quaternion leftArmRest;
        Quaternion rightArmRest;
        float phase;

        void Start()
        {
            lastWorldPos = transform.position;
            if (body) bodyRestEuler = body.localEulerAngles;
            if (head) headRestLocal = head.localPosition;
            if (leftArm) leftArmRest = leftArm.localRotation;
            if (rightArm) rightArmRest = rightArm.localRotation;
        }

        void LateUpdate()
        {
            // Estimate movement speed from world delta — works regardless of
            // whether motion is driven by CharacterController, Rigidbody, etc.
            Vector3 delta = transform.position - lastWorldPos;
            delta.y = 0f;
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            lastWorldPos = transform.position;

            float intensity = Mathf.Clamp01(speed / 3f);
            phase += Time.deltaTime * (swaySpeed * (0.6f + intensity));

            float swaySin = Mathf.Sin(phase) * swayAmount * intensity;
            float bobSin = Mathf.Abs(Mathf.Sin(phase * (bobSpeed / swaySpeed))) * bobAmount * intensity;
            float armSin = Mathf.Sin(phase) * armSwing * intensity;

            if (body)
                body.localRotation = Quaternion.Euler(
                    bodyRestEuler.x,
                    bodyRestEuler.y,
                    bodyRestEuler.z + swaySin);

            if (head)
                head.localPosition = headRestLocal + Vector3.up * bobSin;

            if (leftArm)
                leftArm.localRotation = leftArmRest * Quaternion.Euler(armSin, 0f, 0f);
            if (rightArm)
                rightArm.localRotation = rightArmRest * Quaternion.Euler(-armSin, 0f, 0f);
        }
    }
}
