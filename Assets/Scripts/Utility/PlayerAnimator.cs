using UnityEngine;
using ZombieLand.Player;

namespace ZombieLand.Utility
{
    /// <summary>
    /// Procedural locomotion polish for the player rig: a vertical head/torso
    /// bob synced to footsteps, plus a slight forward lean while sprinting.
    /// Nothing here changes the CharacterController — we only animate the
    /// visual children.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        public Transform rig;     // visual parent we tilt for the lean
        public Transform head;
        public Transform torso;

        public float walkBobSpeed = 7f;
        public float walkBobAmount = 0.06f;
        public float sprintBobMultiplier = 1.4f;
        public float leanAngle = 8f;     // degrees forward when sprinting

        PlayerController controller;
        Vector3 headRest;
        Vector3 torsoRest;
        Quaternion rigRest;
        float phase;

        void Start()
        {
            controller = GetComponent<PlayerController>();
            if (head) headRest = head.localPosition;
            if (torso) torsoRest = torso.localPosition;
            if (rig) rigRest = rig.localRotation;
        }

        void LateUpdate()
        {
            float bobSpeed = walkBobSpeed;
            float bobAmount = walkBobAmount;
            bool moving = controller != null && controller.IsMoving;
            bool sprinting = controller != null && controller.IsSprinting;

            if (sprinting)
            {
                bobSpeed *= sprintBobMultiplier;
                bobAmount *= sprintBobMultiplier;
            }

            phase += Time.deltaTime * bobSpeed;
            float bob = moving ? Mathf.Abs(Mathf.Sin(phase)) * bobAmount : 0f;

            if (head) head.localPosition = headRest + Vector3.up * bob;
            if (torso) torso.localPosition = torsoRest + Vector3.up * bob * 0.6f;

            if (rig)
            {
                float targetLean = sprinting ? leanAngle : 0f;
                Quaternion target = rigRest * Quaternion.Euler(targetLean, 0f, 0f);
                rig.localRotation = Quaternion.Slerp(rig.localRotation, target, 8f * Time.deltaTime);
            }
        }
    }
}
