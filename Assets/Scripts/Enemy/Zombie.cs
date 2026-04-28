using System.Collections.Generic;
using UnityEngine;
using ZombieLand.Pathfinding;
using ZombieLand.Player;
using ZombieLand.UI;
using ZombieLand.Utility;

namespace ZombieLand.Enemy
{
    /// <summary>
    /// A simple finite-state-machine zombie:
    ///   Wander: pick a random reachable cell and shamble toward it.
    ///   Chase:  recompute an A* path to the player every `repathInterval`s.
    /// State transitions are based on distance to the player; the flashlight
    /// extends the zombie's "sight" because the player's light gives them away.
    ///
    /// On contact with the player, the zombie does NOT damage the player.
    /// Instead it triggers a small "memory disturbance" effect: a brief camera
    /// shake and an on-screen message. This is the project's twist on
    /// traditional zombie-game collision feedback.
    /// </summary>
    public class Zombie : MonoBehaviour
    {
        public float wanderSpeed = 1.4f;
        public float chaseSpeed = 2.8f;
        public float sightRange = 9f;
        public float loseSightRange = 14f;
        public float repathInterval = 0.4f;
        public float wanderRadius = 7f;
        public float waypointReachedDist = 0.45f;
        public float disturbCooldown = 1.5f;

        Transform player;
        PlayerFlashlight playerFlashlight;
        Animator animator;

        List<Vector3> currentPath;
        int pathIndex;
        float nextRepathTime;

        Vector3 wanderTarget;
        float nextWanderPickTime;

        float lastDisturbTime = -10f;

        enum State { Wander, Chase }
        State state = State.Wander;

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p)
            {
                player = p.transform;
                playerFlashlight = p.GetComponent<PlayerFlashlight>();
            }
            animator = GetComponentInChildren<Animator>();
            PickNewWanderTarget();
        }

        void Update()
        {
            if (player == null) return;

            UpdateState();
            UpdatePath();
            FollowPath();
            UpdateAnimator();
        }

        void UpdateState()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // The flashlight makes you easier to spot.
            float effectiveSight = sightRange;
            if (playerFlashlight != null && playerFlashlight.On) effectiveSight *= 1.5f;

            if (state == State.Wander && distance <= effectiveSight)
                state = State.Chase;
            else if (state == State.Chase && distance > loseSightRange)
                state = State.Wander;
        }

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
                    Vector3.Distance(transform.position, wanderTarget) < 1f)
                    PickNewWanderTarget();
                target = wanderTarget;
            }

            currentPath = AStarPathfinder.FindPath(transform.position, target);
            pathIndex = 0;
        }

        void FollowPath()
        {
            if (currentPath == null || pathIndex >= currentPath.Count) return;

            Vector3 waypoint = currentPath[pathIndex];
            waypoint.y = transform.position.y;

            Vector3 dir = waypoint - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;
            if (dist < waypointReachedDist)
            {
                pathIndex++;
                return;
            }

            dir /= dist;
            float speed = state == State.Chase ? chaseSpeed : wanderSpeed;
            transform.position += dir * speed * Time.deltaTime;

            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 8f * Time.deltaTime);
        }

        void UpdateAnimator()
        {
            if (animator == null) return;
            float speed = state == State.Chase ? chaseSpeed : wanderSpeed;
            bool moving = currentPath != null && pathIndex < currentPath.Count;
            animator.SetFloat("Speed", moving ? speed : 0f);
            animator.SetBool("Chasing", state == State.Chase);
        }

        void PickNewWanderTarget()
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            wanderTarget = transform.position + new Vector3(r.x, 0f, r.y);
            nextWanderPickTime = Time.time + Random.Range(3f, 6f);
        }

        // Triggered when the player's CharacterController enters this zombie's
        // trigger collider. We deliberately do NOT damage the player; instead
        // we emit cosmetic feedback that fits the "memory" theme.
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            DisturbMemory();
        }

        void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (Time.time - lastDisturbTime < disturbCooldown) return;
            DisturbMemory();
        }

        void DisturbMemory()
        {
            lastDisturbTime = Time.time;
            if (SmoothFollowCamera.Instance != null)
                SmoothFollowCamera.Instance.Shake(0.25f, 0.25f);
            if (HUDController.Instance != null)
                HUDController.Instance.ShowMessage("...a memory flickers...", 1.2f);
        }
    }
}
