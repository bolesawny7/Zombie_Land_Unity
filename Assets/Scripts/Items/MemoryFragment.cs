using UnityEngine;
using ZombieLand.Managers;
using ZombieLand.Player;

namespace ZombieLand.Items
{
    /// <summary>
    /// A collectible glowing orb. Bobs up and down and rotates for visual
    /// feedback. When the player walks into it, the carried memory is
    /// reported back to the GameManager, which surfaces it on the HUD.
    /// </summary>
    public class MemoryFragment : MonoBehaviour
    {
        [TextArea]
        public string memoryText = "I remember the rain on the rooftop...";

        public float bobSpeed = 2f;
        public float bobHeight = 0.25f;
        public float rotateSpeed = 70f;

        Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var stats = other.GetComponent<PlayerStats>();
            if (stats != null) stats.CollectFragment(memoryText);

            if (GameManager.Instance != null)
                GameManager.Instance.OnFragmentCollected(memoryText);

            Destroy(gameObject);
        }
    }
}
