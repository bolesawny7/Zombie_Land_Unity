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
    /// Path-following zombie with three variants. Wanders randomly while
    /// the player is far away and switches to A*-driven chase when
    /// the player enters its sight range.
    ///
    /// Movement is driven by a CharacterController (so walls actually
    /// stop the zombie), and waypoint logic is intentionally simple:
    /// each frame we either advance to the next waypoint or step toward
    /// the current one. Path is recomputed every <see cref="repathInterval"/>s.
    ///
    /// On contact with the player NO damage is dealt; the zombie only
    /// triggers a "memory disturbance" (camera shake + HUD message) —
    /// this is the project's twist on traditional zombie collision.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Zombie : MonoBehaviour
    {
        public ZombieType type = ZombieType.Walker;

        // Filled in by ApplyTypeStats so the variant always wins over
        // whatever the inspector defaults are.
        public float wanderSpeed;
        public float chaseSpeed;
        public float sightRange;
        public float loseSightRange;

        [Header("Pathfinding tuning")]
        public float repathInterval = 0.3f;
        public float waypointReachedDist = 0.5f;
        public float wanderRadius = 6f;

        [Header("Player contact")]
        // Tightened: only fires when the zombie is actually pressed up
        // against the player, not just nearby.
        public float disturbDistance = 1.1f;
        public float disturbCooldown = 1.5f;

        public float gravity = -20f;

        [Header("Damage")]
        public float disturbSoulDamage = 12f;

        Transform player;
        PlayerFlashlight playerFlashlight;
        PlayerStats playerStats;
        CharacterController cc;

        List<Vector3> currentPath = new List<Vector3>();
        int pathIndex;
        float nextRepathTime;

        Vector3 wanderTarget;
        float nextWanderPickTime;

        float lastDisturbTime = -10f;
        Vector3 verticalVel;

        bool isDying;
        float deathStart;
        const float DeathDuration = 1.0f;

        enum State { Wander, Chase }
        State state = State.Wander;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            ApplyTypeStats();
        }

        public void ApplyTypeStats()
        {
            // Sight ranges are deliberately conservative: spawn-camp aggro
            // (zombie chases you the moment you click "Begin") was the cause
            // of the "Soul bar drains on its own" complaint -- a Runner with
            // a 12-unit sight could close on you before you noticed it.
            switch (type)
            {
                case ZombieType.Walker:
                    wanderSpeed = 1.4f;
                    chaseSpeed = 2.6f;
                    sightRange = 6f;
                    loseSightRange = 10f;
                    break;
                case ZombieType.Runner:
                    wanderSpeed = 2.2f;
                    chaseSpeed = 5.2f;
                    sightRange = 8f;
                    loseSightRange = 12f;
                    break;
                case ZombieType.Brute:
                    wanderSpeed = 1.0f;
                    chaseSpeed = 1.9f;
                    sightRange = 5f;
                    loseSightRange = 9f;
                    break;
            }
        }

        void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerFlashlight = p.GetComponent<PlayerFlashlight>();
                playerStats = p.GetComponent<PlayerStats>();
            }
            // Stagger initial repath so all zombies don't search on the same frame.
            nextRepathTime = Time.time + Random.Range(0f, repathInterval);
            PickNewWanderTarget();
        }

        void Update()
        {
            if (isDying)
            {
                UpdateDeath();
                return;
            }

            if (player == null) return;

            UpdateState();
            MaybeRepath();
            Vector3 dir = StepAlongPath();
            ApplyMovement(dir);
            CheckMemoryDisturbance();
        }

        // ----- Death (called by Explosion) -----

        public void Die()
        {
            if (isDying) return;
            isDying = true;
            deathStart = Time.time;
            // Stop blocking the player physically the instant we die.
            if (cc != null) cc.enabled = false;
        }

        void UpdateDeath()
        {
            float t = Mathf.Clamp01((Time.time - deathStart) / DeathDuration);
            // Collapse straight down and shrink.
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, t);
            transform.position += Vector3.down * 0.5f * Time.deltaTime;
            if (t >= 1f) Destroy(gameObject);
        }

        // ----- State machine -----

        void UpdateState()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            float effectiveSight = sightRange;
            if (playerFlashlight != null && playerFlashlight.On)
                effectiveSight += 2f; // your light gives you away (additive, not 1.5x)

            if (state == State.Wander && distance <= effectiveSight)
                state = State.Chase;
            else if (state == State.Chase && distance > loseSightRange)
                state = State.Wander;
        }

        // ----- Pathfinding -----

        void MaybeRepath()
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

            List<Vector3> newPath = AStarPathfinder.FindPath(transform.position, target);
            if (newPath != null && newPath.Count > 0)
            {
                currentPath = newPath;
                pathIndex = 0;
            }
            else if (state == State.Wander)
            {
                // Bad wander pick — reset target so we try again next frame.
                PickNewWanderTarget();
                currentPath.Clear();
                pathIndex = 0;
            }
            // If chase pathfinding returns nothing, keep last known path —
            // it's better than freezing on the spot.
        }

        Vector3 StepAlongPath()
        {
            // Skip waypoints we are already on top of (can happen the very
            // first frame after a repath when the start cell == waypoint[0]).
            while (pathIndex < currentPath.Count)
            {
                Vector3 wp = currentPath[pathIndex];
                Vector3 toWp = new Vector3(wp.x - transform.position.x, 0f, wp.z - transform.position.z);
                if (toWp.magnitude < waypointReachedDist)
                {
                    pathIndex++;
                    continue;
                }
                return toWp.normalized;
            }
            return Vector3.zero;
        }

        // ----- Movement -----

        void ApplyMovement(Vector3 dir)
        {
            float speed = state == State.Chase ? chaseSpeed : wanderSpeed;
            Vector3 horizontal = dir * speed;

            if (cc.isGrounded && verticalVel.y < 0f) verticalVel.y = -1f;
            verticalVel.y += gravity * Time.deltaTime;

            cc.Move((horizontal + verticalVel) * Time.deltaTime);

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 8f * Time.deltaTime);
            }
        }

        // ----- Wander -----

        void PickNewWanderTarget()
        {
            // Try a few times to land on a walkable cell so we don't waste
            // path requests on targets that are inside walls.
            var grid = PathfindingGrid.Instance;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 r = Random.insideUnitCircle * wanderRadius;
                Vector3 candidate = transform.position + new Vector3(r.x, 0f, r.y);
                if (grid == null || grid.NodeFromWorldPoint(candidate).walkable)
                {
                    wanderTarget = candidate;
                    nextWanderPickTime = Time.time + Random.Range(3f, 6f);
                    return;
                }
            }
            // Give up this frame; next Update will retry.
            wanderTarget = transform.position;
            nextWanderPickTime = Time.time + 0.5f;
        }

        // ----- Player contact -----

        void CheckMemoryDisturbance()
        {
            if (Time.time - lastDisturbTime < disturbCooldown) return;
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < disturbDistance)
            {
                lastDisturbTime = Time.time;
                if (SmoothFollowCamera.Instance != null)
                    SmoothFollowCamera.Instance.Shake(0.6f, 0.4f);
                if (HUDController.Instance != null)
                    HUDController.Instance.ShowMessage("They touched you.", 0.9f);
                if (playerStats != null)
                    playerStats.DisturbSoul(disturbSoulDamage);
            }
        }

        // Visualises the zombie's current path in the Scene view so we can
        // verify A* is actually steering toward the player. Red = chase,
        // yellow = wander.
        void OnDrawGizmos()
        {
            if (currentPath == null || currentPath.Count == 0) return;
            Gizmos.color = state == State.Chase ? Color.red : Color.yellow;
            Vector3 prev = transform.position + Vector3.up * 0.5f;
            for (int i = pathIndex; i < currentPath.Count; i++)
            {
                Vector3 wp = currentPath[i] + Vector3.up * 0.5f;
                Gizmos.DrawLine(prev, wp);
                Gizmos.DrawWireSphere(wp, 0.15f);
                prev = wp;
            }
        }
    }
}
