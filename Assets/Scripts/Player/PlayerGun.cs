using UnityEngine;
using ZombieLand.Enemy;
using ZombieLand.UI;

namespace ZombieLand.Player
{
    /// <summary>
    /// "Light-Burst Gun." Left mouse fires a forward raycast that briefly
    /// renders a beam (LineRenderer + muzzle point light) and stuns any
    /// zombie it hits. Stun is non-lethal — true to the project's
    /// no-damage Memory theme — but it gives the player a real verb
    /// other than "run".
    /// </summary>
    public class PlayerGun : MonoBehaviour
    {
        public Transform muzzle;
        public LineRenderer beam;
        public Light muzzleFlash;

        public float fireCooldown = 0.4f;
        public float range = 25f;
        public float stunDuration = 3f;
        public float beamShowTime = 0.08f;

        public LayerMask hitMask = ~0;

        float nextFireTime;
        float beamHideAt;

        void Awake()
        {
            if (beam != null) beam.enabled = false;
            if (muzzleFlash != null) muzzleFlash.enabled = false;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                Fire();

            if (beamHideAt > 0f && Time.time >= beamHideAt)
            {
                if (beam != null) beam.enabled = false;
                if (muzzleFlash != null) muzzleFlash.enabled = false;
                beamHideAt = 0f;
            }
        }

        void Fire()
        {
            nextFireTime = Time.time + fireCooldown;

            Vector3 origin = muzzle != null
                ? muzzle.position
                : transform.position + Vector3.up * 1.2f + transform.forward * 0.7f;
            Vector3 dir = transform.forward;
            Vector3 endPoint = origin + dir * range;

            // Ignore self / the player's own CharacterController.
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.root == transform.root)
                {
                    // We somehow hit ourselves — shoot through.
                }
                else
                {
                    endPoint = hit.point;
                    Zombie z = hit.collider.GetComponentInParent<Zombie>();
                    if (z != null)
                    {
                        z.Stun(stunDuration);
                        if (HUDController.Instance != null)
                            HUDController.Instance.ShowMessage("Memory pinned.", 0.6f);
                    }
                }
            }

            ShowBeam(origin, endPoint);
        }

        void ShowBeam(Vector3 from, Vector3 to)
        {
            if (beam != null)
            {
                beam.enabled = true;
                beam.SetPosition(0, from);
                beam.SetPosition(1, to);
            }
            if (muzzleFlash != null) muzzleFlash.enabled = true;
            beamHideAt = Time.time + beamShowTime;
        }
    }
}
