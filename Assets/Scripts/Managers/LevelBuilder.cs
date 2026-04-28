using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZombieLand.Enemy;
using ZombieLand.Environment;
using ZombieLand.Items;
using ZombieLand.Pathfinding;
using ZombieLand.Player;
using ZombieLand.UI;
using ZombieLand.Utility;

namespace ZombieLand.Managers
{
    /// <summary>
    /// Builds the entire game world (floor, walls, lights, player, zombies,
    /// memory fragments, exit, pathfinding grid, camera, and UI) at runtime
    /// from <see cref="MazeData"/>. Drop one empty GameObject with this script
    /// into a fresh scene and press Play -- nothing else is required.
    ///
    /// Why procedural? It keeps the entire game in source-controlled .cs
    /// files (no fragile .unity scene files), makes the project easy to
    /// re-grade on a fresh machine, and gives us a single readable place
    /// to see how everything is wired together.
    /// </summary>
    public class LevelBuilder : MonoBehaviour
    {
        [Header("World")]
        public float cellSize = 2f;
        public float wallHeight = 3f;

        [Header("Layers")]
        public string obstacleLayerName = "Default"; // walls block pathfinding sight checks

        [Header("Memory texts (in spawn order)")]
        [TextArea] public string[] memories = new[]
        {
            "I remember the rain on the rooftop, the smell of paper and tea.",
            "A song my sister hummed when the lights went out.",
            "The lighthouse keeper's lantern, swinging in the storm.",
            "The first letter I wrote that no one ever read.",
            "Her voice, calling me home through the fog.",
        };

        // References built up while constructing the world.
        Transform worldRoot;
        Transform player;
        readonly List<Vector3> fragmentSpawns = new List<Vector3>();
        readonly List<Vector3> zombieSpawns = new List<Vector3>();
        Vector3 playerSpawn;
        Vector3 exitSpawn;
        bool hasExit;

        int gridCols;
        int gridRows;

        void Awake()
        {
            BuildAll();
        }

        void BuildAll()
        {
            string[] layout = MazeData.Layout;
            gridRows = layout.Length;
            gridCols = layout[0].Length;

            worldRoot = new GameObject("World").transform;

            CreateLighting();
            CreateFog();
            CreateFloor();
            ParseLayoutAndBuild(layout);

            EnsureGameManager();
            BuildPathfindingGrid();
            Physics.SyncTransforms();
            PathfindingGrid.Instance.BuildGrid();

            SpawnPlayer();
            SpawnExit();
            SpawnFragments();
            SpawnZombies();

            BuildCamera();
            BuildUI();
        }

        // -------- World construction --------

        void CreateLighting()
        {
            GameObject moon = new GameObject("MoonLight");
            moon.transform.SetParent(worldRoot);
            moon.transform.rotation = Quaternion.Euler(40f, 35f, 0f);
            Light l = moon.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.55f, 0.65f, 0.95f);
            l.intensity = 0.35f;
            l.shadows = LightShadows.Soft;

            // A subtle warm fill light to keep the scene readable.
            GameObject fill = new GameObject("FillLight");
            fill.transform.SetParent(worldRoot);
            fill.transform.rotation = Quaternion.Euler(-30f, -120f, 0f);
            Light f = fill.AddComponent<Light>();
            f.type = LightType.Directional;
            f.color = new Color(0.9f, 0.6f, 0.4f);
            f.intensity = 0.15f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.1f);
        }

        void CreateFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.08f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.045f;
        }

        void CreateFloor()
        {
            float worldWidth = gridCols * cellSize;
            float worldDepth = gridRows * cellSize;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(worldRoot);
            // Plane is 10x10, so divide world size by 10 for scale.
            floor.transform.localScale = new Vector3(worldWidth / 10f, 1f, worldDepth / 10f);
            floor.transform.position = Vector3.zero;
            ApplyMaterial(floor.GetComponent<Renderer>(), new Color(0.08f, 0.09f, 0.12f), 0.05f, 0.85f);
        }

        void ParseLayoutAndBuild(string[] layout)
        {
            for (int row = 0; row < gridRows; row++)
            {
                string line = layout[row];
                for (int col = 0; col < gridCols; col++)
                {
                    char c = line[col];
                    Vector3 pos = GridToWorld(col, row);

                    switch (c)
                    {
                        case '#': SpawnWall(pos); break;
                        case 'P': playerSpawn = pos; break;
                        case 'E': exitSpawn = pos; hasExit = true; break;
                        case 'F': fragmentSpawns.Add(pos); break;
                        case 'Z': zombieSpawns.Add(pos); break;
                    }
                }
            }
        }

        // ASCII row 0 is at the top of the layout, which we map to the largest
        // Z value so the world reads "north = top of screen".
        Vector3 GridToWorld(int col, int row)
        {
            float worldWidth = gridCols * cellSize;
            float worldDepth = gridRows * cellSize;
            float x = col * cellSize - worldWidth * 0.5f + cellSize * 0.5f;
            float z = (gridRows - 1 - row) * cellSize - worldDepth * 0.5f + cellSize * 0.5f;
            return new Vector3(x, 0f, z);
        }

        void SpawnWall(Vector3 pos)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(worldRoot);
            wall.transform.position = pos + Vector3.up * (wallHeight * 0.5f);
            wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
            ApplyMaterial(wall.GetComponent<Renderer>(), new Color(0.18f, 0.2f, 0.22f), 0.1f, 0.6f);
        }

        // -------- Pathfinding --------

        void BuildPathfindingGrid()
        {
            GameObject gridGO = new GameObject("PathfindingGrid");
            gridGO.transform.SetParent(worldRoot);
            // Sample the grid at y = 1 so we hit wall colliders (which span y = 0..3)
            // but NOT the floor plane sitting at y = 0.
            gridGO.transform.position = new Vector3(0f, 1f, 0f);
            var pf = gridGO.AddComponent<PathfindingGrid>();
            pf.gridWorldSize = new Vector2(gridCols * cellSize, gridRows * cellSize);
            pf.nodeRadius = 0.5f;
            pf.obstacleMask = LayerMask.GetMask(obstacleLayerName);
        }

        // -------- Spawns --------

        void SpawnPlayer()
        {
            GameObject p = new GameObject("Player");
            p.tag = "Player";
            p.transform.SetParent(worldRoot);
            p.transform.position = playerSpawn + Vector3.up * 1f;

            var cc = p.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Visual: capsule body + smaller head sphere for a "soul" silhouette.
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(p.transform, false);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            ApplyMaterial(body.GetComponent<Renderer>(), new Color(0.65f, 0.8f, 1f), 0.2f, 0.4f, emission: new Color(0.15f, 0.2f, 0.4f));

            // Flashlight as a child spotlight.
            GameObject lightGO = new GameObject("Flashlight");
            lightGO.transform.SetParent(p.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            lightGO.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            Light spot = lightGO.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 12f;
            spot.spotAngle = 55f;
            spot.intensity = 4f;
            spot.color = new Color(1f, 0.92f, 0.75f);
            spot.shadows = LightShadows.Soft;

            // A faint personal point light so the player is never invisible.
            GameObject halo = new GameObject("Halo");
            halo.transform.SetParent(p.transform, false);
            halo.transform.localPosition = new Vector3(0f, 1f, 0f);
            Light haloLight = halo.AddComponent<Light>();
            haloLight.type = LightType.Point;
            haloLight.range = 3f;
            haloLight.intensity = 0.7f;
            haloLight.color = new Color(0.5f, 0.7f, 1f);

            p.AddComponent<PlayerController>();
            var fl = p.AddComponent<PlayerFlashlight>();
            fl.flashlight = spot;
            p.AddComponent<PlayerStats>();

            player = p.transform;
        }

        void SpawnExit()
        {
            if (!hasExit)
            {
                Debug.LogWarning("LevelBuilder: no 'E' tile found in MazeData; skipping exit spawn.");
                return;
            }

            GameObject e = new GameObject("ExitPortal");
            e.transform.SetParent(worldRoot);
            e.transform.position = exitSpawn;

            // Visible disc.
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(disc.GetComponent<Collider>());
            disc.transform.SetParent(e.transform, false);
            disc.transform.localPosition = Vector3.up * 0.05f;
            disc.transform.localScale = new Vector3(1.4f, 0.05f, 1.4f);
            ApplyMaterial(disc.GetComponent<Renderer>(), new Color(1f, 0.95f, 0.7f), 0f, 0f,
                emission: new Color(2.5f, 2.2f, 1.5f));

            // Rising column light.
            GameObject lightGO = new GameObject("ExitGlow");
            lightGO.transform.SetParent(e.transform, false);
            lightGO.transform.localPosition = Vector3.up * 1.5f;
            Light glow = lightGO.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 10f;
            glow.color = new Color(1f, 0.92f, 0.65f);
            glow.intensity = 2.5f;

            BoxCollider trigger = e.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.4f, 2f, 1.4f);
            trigger.center = new Vector3(0f, 1f, 0f);

            var portal = e.AddComponent<ExitPortal>();
            portal.glow = glow;
        }

        void SpawnFragments()
        {
            for (int i = 0; i < fragmentSpawns.Count; i++)
            {
                GameObject orb = new GameObject($"MemoryFragment_{i}");
                orb.transform.SetParent(worldRoot);
                orb.transform.position = fragmentSpawns[i] + Vector3.up * 1.0f;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(visual.GetComponent<Collider>());
                visual.transform.SetParent(orb.transform, false);
                visual.transform.localScale = Vector3.one * 0.45f;
                ApplyMaterial(visual.GetComponent<Renderer>(),
                    new Color(0.7f, 0.85f, 1f), 0f, 0.1f,
                    emission: new Color(0.4f, 0.7f, 1.2f));

                GameObject lightGO = new GameObject("Glow");
                lightGO.transform.SetParent(orb.transform, false);
                Light pl = lightGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(0.6f, 0.8f, 1f);
                pl.intensity = 1.5f;
                pl.range = 4f;

                SphereCollider sc = orb.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 0.7f;

                var frag = orb.AddComponent<MemoryFragment>();
                frag.memoryText = (i < memories.Length)
                    ? memories[i]
                    : "A memory you cannot quite name...";
            }

            // Tell the GameManager how many fragments we actually placed so the
            // win condition matches the level data, not the inspector default.
            if (GameManager.Instance != null)
                GameManager.Instance.totalFragments = fragmentSpawns.Count;
        }

        void SpawnZombies()
        {
            foreach (Vector3 pos in zombieSpawns)
            {
                GameObject z = new GameObject("Zombie");
                z.transform.SetParent(worldRoot);
                z.transform.position = pos + Vector3.up * 1f;

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Object.Destroy(body.GetComponent<Collider>());
                body.transform.SetParent(z.transform, false);
                body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
                body.transform.localPosition = new Vector3(0f, 0f, 0f);
                ApplyMaterial(body.GetComponent<Renderer>(),
                    new Color(0.25f, 0.4f, 0.2f), 0.05f, 0.7f);

                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(head.GetComponent<Collider>());
                head.transform.SetParent(z.transform, false);
                head.transform.localScale = Vector3.one * 0.55f;
                head.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                ApplyMaterial(head.GetComponent<Renderer>(),
                    new Color(0.55f, 0.6f, 0.4f), 0.05f, 0.6f);

                // Glowing red eyes — pure cosmetic but adds atmosphere.
                CreateEye(head.transform, new Vector3(0.18f, 0.05f, 0.42f));
                CreateEye(head.transform, new Vector3(-0.18f, 0.05f, 0.42f));

                CapsuleCollider trigger = z.AddComponent<CapsuleCollider>();
                trigger.isTrigger = true;
                trigger.height = 2f;
                trigger.radius = 0.5f;
                trigger.center = new Vector3(0f, 0.5f, 0f);

                var rb = z.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                z.AddComponent<Zombie>();
            }
        }

        void CreateEye(Transform parent, Vector3 localPos)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(eye.GetComponent<Collider>());
            eye.transform.SetParent(parent, false);
            eye.transform.localScale = Vector3.one * 0.18f;
            eye.transform.localPosition = localPos;
            ApplyMaterial(eye.GetComponent<Renderer>(), Color.red, 0f, 0f,
                emission: new Color(2f, 0.1f, 0.1f));
        }

        // -------- Camera & UI --------

        void EnsureGameManager()
        {
            if (GameManager.Instance == null)
                gameObject.AddComponent<GameManager>();
        }

        void BuildCamera()
        {
            GameObject camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f);
            cam.fieldOfView = 55f;
            camGO.AddComponent<AudioListener>();
            var follow = camGO.AddComponent<SmoothFollowCamera>();
            follow.target = player;
        }

        void BuildUI()
        {
            GameObject uiGO = new GameObject("UI");
            uiGO.transform.SetParent(worldRoot);

            UIBuilder.Build(uiGO.transform, player);
        }

        // -------- Material helper --------

        static Material sharedShader;

        // Creates a per-renderer Standard-shader material because we have many
        // colors. This is fine for a small project; for larger scenes one would
        // batch into shared materials.
        static void ApplyMaterial(Renderer r, Color albedo, float metallic, float smoothness, Color? emission = null)
        {
            if (sharedShader == null) sharedShader = new Material(Shader.Find("Standard"));
            Material mat = new Material(sharedShader.shader);
            mat.color = albedo;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            r.sharedMaterial = mat;
        }
    }
}
