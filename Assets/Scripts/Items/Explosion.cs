using UnityEngine;
using ZombieLand.Enemy;

namespace ZombieLand.Items
{
    /// <summary>
    /// Spawns a brief expanding sphere with a point light, kills any
    /// zombies whose colliders fall inside <see cref="killRadius"/>, and
    /// destroys itself when the animation finishes.
    /// </summary>
    public class Explosion : MonoBehaviour
    {
        public float killRadius = 6f;
        public float lifeTime = 0.6f;
        public float maxScale = 12f;

        Transform shellTransform;
        Light pointLight;
        MeshRenderer shellRenderer;
        float startTime;
        bool damageApplied;

        public void Configure(Transform shell, Light light, MeshRenderer renderer)
        {
            shellTransform = shell;
            pointLight = light;
            shellRenderer = renderer;
            startTime = Time.time;
        }

        void Update()
        {
            float t = Mathf.Clamp01((Time.time - startTime) / lifeTime);

            if (shellTransform != null)
            {
                float scale = Mathf.Lerp(0.2f, maxScale, t);
                shellTransform.localScale = Vector3.one * scale;
            }

            if (shellRenderer != null && shellRenderer.material != null)
            {
                Color c = shellRenderer.material.color;
                c.a = Mathf.Lerp(0.85f, 0f, t);
                shellRenderer.material.color = c;
                shellRenderer.material.SetColor("_EmissionColor",
                    new Color(2.5f * (1f - t), 0.6f * (1f - t), 0.2f * (1f - t)));
            }

            if (pointLight != null)
                pointLight.intensity = Mathf.Lerp(8f, 0f, t);

            // Apply damage just once at the start of the explosion,
            // so a zombie sliding into the radius after t=0 isn't killed.
            if (!damageApplied)
            {
                damageApplied = true;
                ApplyDamage();
            }

            if (t >= 1f) Destroy(gameObject);
        }

        void ApplyDamage()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, killRadius);
            foreach (Collider c in hits)
            {
                Zombie z = c.GetComponentInParent<Zombie>();
                if (z != null) z.Die();
            }
        }
    }
}
