using System.Collections.Generic;
using UnityEngine;
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
    /// Builds the entire game world (floor, walls, lights, player + gun,
    /// zombies of three variants, memory fragments, exit, pathfinding grid,
    /// camera, and UI) at runtime from <see cref="MazeData"/>. Drop one
    /// empty GameObject with this script into a fresh scene and press Play.
    /// </summary>
    public class LevelBuilder : MonoBehaviour
    {
        [Header("World")]
        public float cellSize = 2f;
        public float wallHeight = 3f;

        [Header("Layers")]
        public string obstacleLayerName = "Default";

        [Header("Memory texts (in spawn order)")]
        [TextArea] public string[] memories = new[]
        {
            "I remember the rain on the rooftop, the smell of paper and tea.",
            "A song my sister hummed when the lights went out.",
            "The lighthouse keeper's lantern, swinging in the storm.",
            "The first letter I wrote that no one ever read.",
            "Her voice, calling me home through the fog.",
        };

        Transform worldRoot;
        Transform player;

        readonly List<Vector3> fragmentSpawns = new List<Vector3>();
        readonly List<(Vector3 pos, ZombieType type)> zombieSpawns = new List<(Vector3, ZombieType)>();
        Vector3 playerSpawn;
        Vector3 exitSpawn;
        bool hasExit;

        int gridCols;
        int gridRows;

        void Awake() => BuildAll();

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
            floor.transform.localScale = new Vector3(worldWidth / 10f, 1f, worldDepth / 10f);
            floor.transform.position = Vector3.zero;
            ApplyMaterial(floor.GetComponent<Renderer>(),
                new Color(0.08f, 0.09f, 0.12f), 0.05f, 0.85f);
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
                        case 'Z': zombieSpawns.Add((pos, ZombieType.Walker)); break;
                        case 'R': zombieSpawns.Add((pos, ZombieType.Runner)); break;
                        case 'B': zombieSpawns.Add((pos, ZombieType.Brute));  break;
                    }
                }
            }
        }

        // ASCII row 0 maps to the LARGEST Z (top of layout = "north").
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
            ApplyMaterial(wall.GetComponent<Renderer>(),
                new Color(0.18f, 0.2f, 0.22f), 0.1f, 0.6f);
        }

        // -------- Pathfinding --------

        void BuildPathfindingGrid()
        {
            GameObject gridGO = new GameObject("PathfindingGrid");
            gridGO.transform.SetParent(worldRoot);
            // Sample at y = 1: above the floor plane, inside wall colliders.
            gridGO.transform.position = new Vector3(0f, 1f, 0f);
            var pf = gridGO.AddComponent<PathfindingGrid>();
            pf.gridWorldSize = new Vector2(gridCols * cellSize, gridRows * cellSize);
            pf.nodeRadius = 0.5f;
            pf.obstacleMask = LayerMask.GetMask(obstacleLayerName);
        }

        // -------- Player --------

        void SpawnPlayer()
        {
            GameObject p = new GameObject("Player");
            p.tag = "Player";
            p.transform.SetParent(worldRoot);
            p.transform.position = playerSpawn;

            CharacterController cc = p.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Cloak / mantle (lower)
            GameObject cloak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(cloak.GetComponent<Collider>());
            cloak.transform.SetParent(p.transform, false);
            cloak.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            cloak.transform.localScale = new Vector3(0.85f, 0.65f, 0.85f);
            ApplyMaterial(cloak.GetComponent<Renderer>(),
                new Color(0.15f, 0.25f, 0.4f), 0.0f, 0.2f,
                emission: new Color(0.05f, 0.08f, 0.18f));

            // Slim torso (upper)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(torso.GetComponent<Collider>());
            torso.transform.SetParent(p.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            torso.transform.localScale = new Vector3(0.55f, 0.4f, 0.55f);
            ApplyMaterial(torso.GetComponent<Renderer>(),
                new Color(0.55f, 0.7f, 1f), 0f, 0.3f,
                emission: new Color(0.15f, 0.25f, 0.45f));

            // Glowing head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(head.GetComponent<Collider>());
            head.transform.SetParent(p.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            head.transform.localScale = Vector3.one * 0.35f;
            ApplyMaterial(head.GetComponent<Renderer>(),
                new Color(0.85f, 0.95f, 1f), 0f, 0.2f,
                emission: new Color(0.45f, 0.65f, 1.2f));

            // Personal halo so the player is never completely invisible.
            GameObject halo = new GameObject("Halo");
            halo.transform.SetParent(p.transform, false);
            halo.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            Light haloLight = halo.AddComponent<Light>();
            haloLight.type = LightType.Point;
            haloLight.range = 4f;
            haloLight.intensity = 0.9f;
            haloLight.color = new Color(0.5f, 0.7f, 1f);

            // Flashlight spotlight.
            GameObject lightGO = new GameObject("Flashlight");
            lightGO.transform.SetParent(p.transform, false);
            lightGO.transform.localPosition = new Vector3(0.0f, 1.7f, 0.3f);
            lightGO.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            Light spot = lightGO.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 12f;
            spot.spotAngle = 55f;
            spot.intensity = 4f;
            spot.color = new Color(1f, 0.92f, 0.75f);
            spot.shadows = LightShadows.Soft;

            p.AddComponent<PlayerController>();
            PlayerFlashlight fl = p.AddComponent<PlayerFlashlight>();
            fl.flashlight = spot;
            p.AddComponent<PlayerStats>();

            player = p.transform;
        }

        // -------- Exit portal --------

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

            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(disc.GetComponent<Collider>());
            disc.transform.SetParent(e.transform, false);
            disc.transform.localPosition = Vector3.up * 0.05f;
            disc.transform.localScale = new Vector3(1.4f, 0.05f, 1.4f);
            ApplyMaterial(disc.GetComponent<Renderer>(),
                new Color(1f, 0.95f, 0.7f), 0f, 0f,
                emission: new Color(2.5f, 2.2f, 1.5f));

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

            ExitPortal portal = e.AddComponent<ExitPortal>();
            portal.glow = glow;
        }

        // -------- Memory fragments --------

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

                MemoryFragment frag = orb.AddComponent<MemoryFragment>();
                frag.memoryText = (i < memories.Length)
                    ? memories[i]
                    : "A memory you cannot quite name...";
            }

            if (GameManager.Instance != null)
                GameManager.Instance.totalFragments = fragmentSpawns.Count;
        }

        // -------- Zombies --------

        void SpawnZombies()
        {
            foreach (var (pos, type) in zombieSpawns)
                BuildZombie(pos, type);
        }

        void BuildZombie(Vector3 worldPos, ZombieType type)
        {
            GameObject z = new GameObject($"Zombie_{type}");
            z.transform.SetParent(worldRoot);
            z.transform.position = worldPos;

            // Visual / physical dimensions per type.
            float radius, height, bodyScaleY, bodyXZ, headScale, eyeScale;
            Color bodyColor, headColor;
            Color emission;
            Color eyeColor;
            float armScale;

            switch (type)
            {
                case ZombieType.Walker:
                    radius = 0.45f; height = 1.8f;
                    bodyScaleY = 0.9f; bodyXZ = 0.85f;
                    headScale = 0.55f; eyeScale = 0.10f;
                    bodyColor  = new Color(0.30f, 0.42f, 0.22f);
                    headColor  = new Color(0.50f, 0.55f, 0.35f);
                    emission   = new Color(0.05f, 0.06f, 0.02f);
                    eyeColor   = new Color(2.5f, 0.2f, 0.1f);
                    armScale = 0.18f;
                    break;
                case ZombieType.Runner:
                    radius = 0.35f; height = 1.8f;
                    bodyScaleY = 0.9f; bodyXZ = 0.65f;
                    headScale = 0.45f; eyeScale = 0.09f;
                    bodyColor  = new Color(0.45f, 0.18f, 0.20f);
                    headColor  = new Color(0.55f, 0.30f, 0.30f);
                    emission   = new Color(0.18f, 0.02f, 0.02f);
                    eyeColor   = new Color(3.5f, 0.25f, 0.15f);
                    armScale = 0.14f;
                    break;
                case ZombieType.Brute:
                default:
                    radius = 0.6f; height = 2.3f;
                    bodyScaleY = 1.1f; bodyXZ = 1.15f;
                    headScale = 0.7f; eyeScale = 0.12f;
                    bodyColor  = new Color(0.18f, 0.18f, 0.20f);
                    headColor  = new Color(0.30f, 0.30f, 0.32f);
                    emission   = new Color(0.04f, 0.04f, 0.05f);
                    eyeColor   = new Color(2.5f, 1.8f, 0.4f);
                    armScale = 0.26f;
                    break;
            }

            CharacterController cc = z.AddComponent<CharacterController>();
            cc.radius = radius;
            cc.height = height;
            cc.center = new Vector3(0f, height * 0.5f, 0f);
            cc.slopeLimit = 60f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(z.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(bodyXZ, bodyScaleY, bodyXZ);
            ApplyMaterial(body.GetComponent<Renderer>(), bodyColor, 0.05f, 0.5f, emission);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(head.GetComponent<Collider>());
            head.transform.SetParent(z.transform, false);
            head.transform.localPosition = new Vector3(0f, height + 0.05f, 0f);
            head.transform.localScale = Vector3.one * headScale;
            ApplyMaterial(head.GetComponent<Renderer>(), headColor, 0.05f, 0.55f, emission * 1.5f);

            BuildArms(z.transform, type, height, armScale, bodyColor, emission);
            BuildEyes(head.transform, eyeScale, eyeColor);

            Zombie zombie = z.AddComponent<Zombie>();
            zombie.type = type;
            zombie.ApplyTypeStats();
        }

        void BuildArms(Transform parent, ZombieType type,
            float height, float thickness, Color color, Color emission)
        {
            float forward = type == ZombieType.Brute ? 0.0f : 0.45f;
            float armLen = type == ZombieType.Brute ? 0.55f : 0.45f;
            float yPos = type == ZombieType.Brute ? height * 0.55f : height * 0.65f;
            float side = type == ZombieType.Runner ? 0.28f : 0.36f;

            MakeArm(parent, new Vector3(-side, yPos, forward), armLen, thickness, color, emission);
            MakeArm(parent, new Vector3( side, yPos, forward), armLen, thickness, color, emission);
        }

        void MakeArm(Transform parent, Vector3 localPos, float length,
            float thickness, Color color, Color emission)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(arm.GetComponent<Collider>());
            arm.transform.SetParent(parent, false);
            arm.transform.localPosition = localPos;
            arm.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
            arm.transform.localScale = new Vector3(thickness, length, thickness);
            ApplyMaterial(arm.GetComponent<Renderer>(), color * 0.85f, 0.05f, 0.5f, emission);
        }

        void BuildEyes(Transform headTransform, float eyeScale, Color emission)
        {
            MakeEye(headTransform, new Vector3(-0.18f, 0.05f, 0.42f), eyeScale, emission);
            MakeEye(headTransform, new Vector3( 0.18f, 0.05f, 0.42f), eyeScale, emission);

            GameObject lightGO = new GameObject("EyeLight");
            lightGO.transform.SetParent(headTransform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.05f, 0.5f);
            Light l = lightGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 2.5f;
            float maxChan = Mathf.Max(emission.r, emission.g, emission.b, 1f);
            l.color = new Color(emission.r / maxChan, emission.g / maxChan, emission.b / maxChan);
            l.intensity = 1.2f;
        }

        void MakeEye(Transform parent, Vector3 localPos, float scale, Color emission)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(eye.GetComponent<Collider>());
            eye.transform.SetParent(parent, false);
            eye.transform.localScale = Vector3.one * scale;
            eye.transform.localPosition = localPos;
            ApplyMaterial(eye.GetComponent<Renderer>(), Color.black, 0f, 0f, emission);
        }

        // -------- Camera, GameManager, UI --------

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
            SmoothFollowCamera follow = camGO.AddComponent<SmoothFollowCamera>();
            follow.target = player;
        }

        void BuildUI()
        {
            GameObject uiGO = new GameObject("UI");
            uiGO.transform.SetParent(worldRoot);
            UIBuilder.Build(uiGO.transform, player);
        }

        // -------- Material helper --------

        static Material sharedShaderProbe;

        // Creates a per-renderer Standard-shader material. For a small
        // project this is fine; for larger scenes one would batch into
        // shared materials.
        static void ApplyMaterial(Renderer r, Color albedo, float metallic, float smoothness, Color? emission = null)
        {
            if (sharedShaderProbe == null) sharedShaderProbe = new Material(Shader.Find("Standard"));
            Material mat = new Material(sharedShaderProbe.shader);
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
