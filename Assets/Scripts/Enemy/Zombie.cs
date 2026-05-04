using System.Collections.Generic;
using UnityEngine;
using ZombieLand.Pathfinding;
using ZombieLand.Player;
using ZombieLand.UI;
using ZombieLand.Utility;

namespace ZombieLand.Enemy
{
    public enum ZombieType { Walker, Runner, Brute }

    /// <summary>
    /// Path-following zombie with three variants and three behavioural states.
    ///
    /// Movement is now driven by a <see cref="CharacterController"/> so the
    /// zombie collides with walls (and the player) properly instead of
    /// teleporting through them — this fixes the previous bug where a
    /// chasing zombie could slide right through geometry.
    ///
    /// On contact with the player the zombie still does NO damage; it
    /// triggers a "memory disturbance" effect (camera shake + HUD flicker).
    /// Player gunfire calls <see cref="Stun"/>, which freezes the zombie in
    /// place for a few seconds and tints it briefly.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Zombie : MonoBehaviour
    {
        public ZombieType type = ZombieType.Walker;

        // Stats are filled in by ApplyTypeStats so the inspector defaults
        // never disagree with the variant we asked for.
        public float wanderSpeed;
        public float chaseSpeed;
        public float sightRange;
        public float loseSightRange;

        public float repathInterval = 0.4f;
        public float wanderRadius = 7f;
        public float waypointReachedDist = 0.45f;

        public float disturbDistance = 1.4f;
        public float disturbCooldown = 1.5f;

        public float gravity = -20f;

        // Renderers tinted while stunned.
        public Renderer[] bodyRenderers;
        public Light[] eyeLights;

        Transform player;
        PlayerFlashlight playerFlashlight;
        CharacterController cc;

        readonly List<Vector3> emptyPath = new List<Vector3>();
        List<Vector3> currentPath;
        int pathIndex;
        float nextRepathTime;

        Vector3 wanderTarget;
        float nextWanderPickTime;

        float lastDisturbTime = -10f;
        float stunUntilTime;
        Color[] originalColors;
        float[] originalEyeIntensity;

        Vector3 verticalVel;

        enum State { Wander, Chase }
        State state = State.Wander;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            ApplyTypeStats();
        }

        public void ApplyTypeStats()
        {
            switch (type)
            {
                case ZombieType.Walker:
                    wanderSpeed = 1.4f;
                    chaseSpeed = 2.6f;
                    sightRange = 9f;
                    loseSightRange = 14f;
                    break;
                case ZombieType.Runner:
                    wanderSpeed = 2.2f;
                    chaseSpeed = 5.2f;
                    sightRange = 12f;
                    loseSightRange = 18f;
                    break;
                case ZombieType.Brute:
                    wanderSpeed = 1.0f;
                    chaseSpeed = 1.9f;
                    sightRange = 7f;
                    loseSightRange = 11f;
                    break;
            }
        }

        void Start()
        {
            CacheVisualOriginals();

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerFlashlight = p.GetComponent<PlayerFlashlight>();
            }
            PickNewWanderTarget();
        }

        void Update()
        {
            if (player == null) return;

            // Stunned: stand still but still apply gravity so the CC doesn't
            // float if the floor under us shifts.
            if (Time.time < stunUntilTime)
            {
                ApplyGravityOnly();
                return;
            }

            // Just exited stun this frame — restore visuals.
            if (originalColors != null && IsTinted())
                RestoreTint();

            UpdateState();
            UpdatePath();
            FollowPath();
            CheckMemoryDisturbance();
        }

        // ----- State machine -----

        void UpdateState()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            float effectiveSight = sightRange;
            if (playerFlashlight != null && playerFlashlight.On)
                effectiveSight *= 1.5f; // your flashlight gives you away

            if (state == State.Wander && distance <= effectiveSight)
                state = State.Chase;
            else if (state == State.Chase && distance > loseSightRange)
                state = State.Wander;
        }

        // ----- Pathfinding -----

        void UpdatePath()
        {
            if (Time.time < nextRepathTime) return;
            nextRepathTime = Time.time + repathInterval;

            Vector3 target;
            if (state == State.Chase)
            {
                target = player.position;
            }
            else
            {
                if (Time.time >= nextWanderPickTime ||
                    Vector3.Distance(transform.position, wanderTarget) < 1.5f)
                    PickNewWanderTarget();
                target = wanderTarget;
            }

            currentPath = AStarPathfinder.FindPath(transform.position, target) ?? emptyPath;
            pathIndex = 0;
        }

        // ----- Movement -----

        void FollowPath()
        {
            Vector3 horizontal = Vector3.zero;

            if (currentPath != null && pathIndex < currentPath.Count)
            {
                Vector3 waypoint = currentPath[pathIndex];

                Vector3 toWaypoint = new Vector3(
                    waypoint.x - transform.position.x,
                    0f,
                    waypoint.z - transform.position.z);
                float dist = toWaypoint.magnitude;

                if (dist < waypointReachedDist)
                {
                    pathIndex++;
                }
                else
                {
                    Vector3 dir = toWaypoint / dist;
                    float speed = state == State.Chase ? chaseSpeed : wanderSpeed;
                    horizontal = dir * speed;

                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 8f * Time.deltaTime);
                }
            }

            // Gravity keeps the CC sitting on the floor regardless of path.
            if (cc.isGrounded && verticalVel.y < 0f) verticalVel.y = -1f;
            verticalVel.y += gravity * Time.deltaTime;

            cc.Move((horizontal + verticalVel) * Time.deltaTime);
        }

        void ApplyGravityOnly()
        {
            if (cc.isGrounded && verticalVel.y < 0f) verticalVel.y = -1f;
            verticalVel.y += gravity * Time.deltaTime;
            cc.Move(verticalVel * Time.deltaTime);
        }

        // ----- Memory disturbance (replaces traditional damage) -----

        void CheckMemoryDisturbance()
        {
            if (Time.time - lastDisturbTime < disturbCooldown) return;
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < disturbDistance)
            {
                lastDisturbTime = Time.time;
                if (SmoothFollowCamera.Instance != null)
                    SmoothFollowCamera.Instance.Shake(0.25f, 0.25f);
                if (HUDController.Instance != null)
                    HUDController.Instance.ShowMessage("...a memory flickers...", 1.2f);
            }
        }

        void PickNewWanderTarget()
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            wanderTarget = transform.position + new Vector3(r.x, 0f, r.y);
            nextWanderPickTime = Time.time + Random.Range(3f, 6f);
        }

        // ----- Stun (called by PlayerGun) -----

        public void Stun(float duration)
        {
            stunUntilTime = Mathf.Max(stunUntilTime, Time.time + duration);
            currentPath = null;
            pathIndex = 0;
            ApplyTint(new Color(0.6f, 0.85f, 1f), 1.5f);
        }

        public bool IsStunned => Time.time < stunUntilTime;

        // ----- Visual tint helpers -----

        void CacheVisualOriginals()
        {
            if (bodyRenderers == null) return;
            originalColors = new Color[bodyRenderers.Length];
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (bodyRenderers[i] != null)
                    originalColors[i] = bodyRenderers[i].material.GetColor("_EmissionColor");
            }
            if (eyeLights != null)
            {
                originalEyeIntensity = new float[eyeLights.Length];
                for (int i = 0; i < eyeLights.Length; i++)
                    if (eyeLights[i] != null) originalEyeIntensity[i] = eyeLights[i].intensity;
            }
        }

        void ApplyTint(Color emissive, float multiplier)
        {
            if (bodyRenderers == null) return;
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (bodyRenderers[i] == null) continue;
                bodyRenderers[i].material.EnableKeyword("_EMISSION");
                bodyRenderers[i].material.SetColor("_EmissionColor", emissive * multiplier);
            }
            if (eyeLights != null)
            {
                for (int i = 0; i < eyeLights.Length; i++)
                    if (eyeLights[i] != null) eyeLights[i].intensity = 0.1f;
            }
        }

        bool IsTinted()
        {
            if (bodyRenderers == null || bodyRenderers.Length == 0 || originalColors == null) return false;
            return bodyRenderers[0] != null &&
                   bodyRenderers[0].material.GetColor("_EmissionColor") != originalColors[0];
        }

        void RestoreTint()
        {
            if (bodyRenderers == null) return;
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (bodyRenderers[i] == null) continue;
                bodyRenderers[i].material.SetColor("_EmissionColor", originalColors[i]);
            }
            if (eyeLights != null && originalEyeIntensity != null)
            {
                for (int i = 0; i < eyeLights.Length; i++)
                    if (eyeLights[i] != null) eyeLights[i].intensity = originalEyeIntensity[i];
            }
        }
    }
}
