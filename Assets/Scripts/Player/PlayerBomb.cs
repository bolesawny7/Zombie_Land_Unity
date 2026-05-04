using UnityEngine;
using ZombieLand.Items;
using ZombieLand.UI;

namespace ZombieLand.Player
{
    /// <summary>
    /// Listens for the bomb keybind and, if the player has at least one bomb
    /// in their inventory, spawns an <see cref="Explosion"/> at the player's
    /// feet that kills any zombie within the explosion radius.
    /// </summary>
    public class PlayerBomb : MonoBehaviour
    {
        public KeyCode bombKey = KeyCode.Space;
        public float killRadius = 6f;
        public float spawnHeight = 0.4f;

        PlayerStats stats;

        void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            if (stats == null) return;
            if (!Input.GetKeyDown(bombKey)) return;

            if (stats.BombCount <= 0)
            {
                HUDController.Instance?.ShowMessage("No bombs.", 0.8f);
                return;
            }

            stats.TryConsumeBomb();
            SpawnExplosion();
            HUDController.Instance?.ShowMessage("BOOM.", 0.6f);
        }

        void SpawnExplosion()
        {
            GameObject root = new GameObject("Explosion");
            root.transform.position = transform.position + Vector3.up * spawnHeight;

            // Visible expanding shell.
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(shell.GetComponent<Collider>());
            shell.transform.SetParent(root.transform, false);
            shell.transform.localScale = Vector3.one * 0.2f;

            Renderer ren = shell.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = new Color(1f, 0.55f, 0.2f, 0.85f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(2.5f, 0.6f, 0.2f));
            ren.sharedMaterial = mat;

            // Punchy point light.
            GameObject lightGO = new GameObject("BoomLight");
            lightGO.transform.SetParent(root.transform, false);
            Light l = lightGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.6f, 0.3f);
            l.range = 12f;
            l.intensity = 8f;

            Explosion ex = root.AddComponent<Explosion>();
            ex.killRadius = killRadius;
            ex.Configure(shell.transform, l, ren as MeshRenderer);
        }
    }
}
