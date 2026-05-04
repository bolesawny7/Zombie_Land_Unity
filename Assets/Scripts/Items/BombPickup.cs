using UnityEngine;
using ZombieLand.Player;
using ZombieLand.UI;

namespace ZombieLand.Items
{
    /// <summary>
    /// A collectible bomb. Walking into it adds one bomb to the player's
    /// inventory; press SPACE to detonate.
    /// </summary>
    public class BombPickup : MonoBehaviour
    {
        public float bobSpeed = 2.5f;
        public float bobHeight = 0.25f;
        public float spinSpeed = 90f;

        Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats == null) return;

            stats.AddBomb();
            HUDController.Instance?.ShowMessage("Bomb collected. (Space)", 1.5f);
            Destroy(gameObject);
        }
    }
}
