using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZombieLand.Player;

namespace ZombieLand.UI
{
    /// <summary>
    /// Builds the Canvas hierarchy at runtime: MainMenu, HUD, PauseMenu,
    /// and Win panel. Wires references up to HUDController and MenuController.
    ///
    /// Keeping this in code (instead of a .prefab) means the entire UI is
    /// reproducible from source files alone -- there's no hidden state in
    /// scene/prefab assets.
    /// </summary>
    public static class UIBuilder
    {
        public static void Build(Transform parent, Transform player)
        {
            EnsureEventSystem();

            GameObject canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(parent, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // ----- Damage flash overlay (drawn first so other UI sits on top) -----
            GameObject flashGO = MakePanel(canvas.transform, "DamageFlash", new Color(1f, 0.1f, 0.1f, 0f));
            Image flashImage = flashGO.GetComponent<Image>();
            flashImage.raycastTarget = false;

            // ----- HUD panel -----
            GameObject hudPanel = MakePanel(canvas.transform, "HUD", new Color(0, 0, 0, 0));
            Text fragmentText = MakeText(hudPanel.transform, "FragmentText",
                "Memories: 0 / 5", new Vector2(40, -40),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 28, TextAnchor.UpperLeft);
            Text messageText = MakeText(hudPanel.transform, "MessageText",
                "", new Vector2(0, 220),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), 30, TextAnchor.MiddleCenter);
            messageText.color = new Color(1f, 0.95f, 0.7f);

            // Soul Integrity (health) bar — largest and most prominent.
            MakeText(hudPanel.transform, "SoulLabel",
                "HEALTH", new Vector2(40, -80),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 20, TextAnchor.UpperLeft).color
                = new Color(1f, 0.6f, 0.6f);
            Image soulFill = MakeBar(hudPanel.transform, "Soul",
                new Vector2(40, -105), new Color(1f, 0.3f, 0.35f), 320, 28);

            // Battery bar (flashlight).
            MakeText(hudPanel.transform, "BatteryLabel",
                "Flashlight (F)", new Vector2(40, -150),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 18, TextAnchor.UpperLeft).color
                = new Color(1, 1, 1, 0.6f);
            Image batteryFill = MakeBar(hudPanel.transform, "Battery",
                new Vector2(40, -175), new Color(1f, 0.85f, 0.5f));

            // Stamina bar (sprint).
            MakeText(hudPanel.transform, "StaminaLabel",
                "Sprint (Shift)", new Vector2(40, -215),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 18, TextAnchor.UpperLeft).color
                = new Color(1, 1, 1, 0.6f);
            Image staminaFill = MakeBar(hudPanel.transform, "Stamina",
                new Vector2(40, -240), new Color(0.6f, 0.85f, 1f));

            // Bombs counter (top-right).
            Text bombText = MakeText(hudPanel.transform, "BombText",
                "Bombs: 0", new Vector2(-40, -40),
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), 28, TextAnchor.UpperRight);
            bombText.color = new Color(1f, 0.7f, 0.55f);

            MakeText(hudPanel.transform, "Hint",
                "WASD: Move    Shift: Sprint    F: Flashlight    Space: Bomb    Mouse: Look    ESC: Pause",
                new Vector2(0, 30),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                17, TextAnchor.MiddleCenter).color = new Color(1, 1, 1, 0.4f);

            HUDController hud = canvasGO.AddComponent<HUDController>();
            hud.fragmentText = fragmentText;
            hud.messageText = messageText;
            hud.bombText = bombText;
            hud.batteryFill = batteryFill;
            hud.staminaFill = staminaFill;
            hud.soulFill = soulFill;
            if (player != null)
                hud.Bind(
                    player.GetComponent<PlayerStats>(),
                    player.GetComponent<PlayerFlashlight>(),
                    player.GetComponent<PlayerController>());

            DamageFlash damageFlash = canvasGO.AddComponent<DamageFlash>();
            damageFlash.overlay = flashImage;
            if (player != null) damageFlash.Bind(player.GetComponent<PlayerStats>());

            // ----- Main Menu -----
            GameObject mainMenu = MakePanel(canvas.transform, "MainMenu", new Color(0, 0, 0, 0.85f));
            MakeText(mainMenu.transform, "Title", "ZOMBIE LAND",
                new Vector2(0, 220),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                72, TextAnchor.MiddleCenter).color = new Color(1f, 0.5f, 0.4f);
            MakeText(mainMenu.transform, "Sub", "Echoes of Memory",
                new Vector2(0, 140),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                28, TextAnchor.MiddleCenter).color = new Color(0.7f, 0.85f, 1f);
            MakeText(mainMenu.transform, "Hint",
                "Collect every fragment of who you used to be, then walk into the Light.",
                new Vector2(0, 60),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                20, TextAnchor.MiddleCenter).color = new Color(1, 1, 1, 0.7f);

            Button startBtn = MakeButton(mainMenu.transform, "StartButton", "Begin", new Vector2(0, -40));
            Button instructionsBtn = MakeButton(mainMenu.transform, "InstructionsButton", "How to Play", new Vector2(0, -120));
            Button quitMain = MakeButton(mainMenu.transform, "QuitButton", "Quit", new Vector2(0, -200));

            // ----- Pause Menu -----
            GameObject pauseMenu = MakePanel(canvas.transform, "PauseMenu", new Color(0, 0, 0, 0.7f));
            MakeText(pauseMenu.transform, "Title", "Paused",
                new Vector2(0, 160),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                56, TextAnchor.MiddleCenter);
            Button resumeBtn = MakeButton(pauseMenu.transform, "ResumeButton", "Resume", new Vector2(0, 40));
            Button restartFromPauseBtn = MakeButton(pauseMenu.transform, "RestartButton", "Restart", new Vector2(0, -40));
            Button quitFromPauseBtn = MakeButton(pauseMenu.transform, "QuitButton", "Quit", new Vector2(0, -120));

            // ----- Win Panel -----
            GameObject winPanel = MakePanel(canvas.transform, "WinPanel", new Color(0, 0, 0, 0.92f));
            MakeText(winPanel.transform, "Title", "You remembered.",
                new Vector2(0, 280),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                64, TextAnchor.MiddleCenter).color = new Color(1f, 0.92f, 0.7f);
            Text summary = MakeText(winPanel.transform, "Summary", "",
                new Vector2(0, 60),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                22, TextAnchor.UpperCenter);
            summary.rectTransform.sizeDelta = new Vector2(900, 320);
            summary.color = new Color(1, 1, 1, 0.85f);

            Button restartFromWinBtn = MakeButton(winPanel.transform, "RestartButton", "Play Again", new Vector2(0, -180));
            Button quitFromWinBtn = MakeButton(winPanel.transform, "QuitButton", "Quit", new Vector2(0, -260));

            // ----- Instructions Panel -----
            GameObject instructionsPanel = MakePanel(canvas.transform, "InstructionsPanel", new Color(0, 0, 0, 0.92f));
            MakeText(instructionsPanel.transform, "Title", "HOW TO PLAY",
                new Vector2(0, 320),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                52, TextAnchor.MiddleCenter).color = new Color(1f, 0.85f, 0.6f);

            string instructions =
                "CONTROLS\n" +
                "  WASD  -  Move\n" +
                "  Shift  -  Sprint (uses stamina)\n" +
                "  F  -  Toggle flashlight\n" +
                "  Space  -  Detonate bomb\n" +
                "  Mouse  -  Rotate camera\n" +
                "  Esc  -  Pause\n\n" +
                "GOAL\n" +
                "  Collect all 5 Memory Fragments (blue orbs)\n" +
                "  then walk into the golden Exit Light.\n\n" +
                "TIPS\n" +
                "  - Your HEALTH bar drops when zombies touch you.\n" +
                "    If it hits zero, you die. Stay away!\n" +
                "  - Bombs kill all zombies in a large radius.\n" +
                "    Pick up red orbs to get bombs.\n" +
                "  - Flashlight helps you see but zombies\n" +
                "    can spot you from further away.\n" +
                "  - Sprint lets you outrun any zombie,\n" +
                "    but watch your stamina bar.";

            Text instrText = MakeText(instructionsPanel.transform, "Body", instructions,
                new Vector2(0, 0),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                22, TextAnchor.MiddleLeft);
            instrText.rectTransform.sizeDelta = new Vector2(700, 500);
            instrText.color = new Color(1, 1, 1, 0.85f);

            Button backFromInstructions = MakeButton(instructionsPanel.transform, "BackButton", "Back", new Vector2(0, -320));

            // ----- Lost Panel -----
            GameObject lostPanel = MakePanel(canvas.transform, "LostPanel", new Color(0.05f, 0f, 0f, 0.92f));
            MakeText(lostPanel.transform, "Title", "The fog took you.",
                new Vector2(0, 220),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                64, TextAnchor.MiddleCenter).color = new Color(1f, 0.4f, 0.4f);
            MakeText(lostPanel.transform, "Sub",
                "Your Soul Integrity reached zero. The memories scatter again.",
                new Vector2(0, 100),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                24, TextAnchor.MiddleCenter).color = new Color(1, 1, 1, 0.7f);
            Button retryFromLostBtn = MakeButton(lostPanel.transform, "RetryButton", "Try Again", new Vector2(0, -40));
            Button quitFromLostBtn = MakeButton(lostPanel.transform, "QuitButton", "Quit", new Vector2(0, -120));

            // ----- Hook them all up -----
            MenuController mc = canvasGO.AddComponent<MenuController>();
            mc.mainMenuPanel = mainMenu;
            mc.pauseMenuPanel = pauseMenu;
            mc.winPanel = winPanel;
            mc.hudPanel = hudPanel;
            mc.startButton = startBtn;
            mc.quitFromMainButton = quitMain;
            mc.resumeButton = resumeBtn;
            mc.restartFromPauseButton = restartFromPauseBtn;
            mc.quitFromPauseButton = quitFromPauseBtn;
            mc.restartFromWinButton = restartFromWinBtn;
            mc.quitFromWinButton = quitFromWinBtn;
            mc.winSummaryText = summary;
            mc.lostPanel = lostPanel;
            mc.retryFromLostButton = retryFromLostBtn;
            mc.quitFromLostButton = quitFromLostBtn;
            mc.instructionsPanel = instructionsPanel;
            mc.instructionsButton = instructionsBtn;
            mc.backFromInstructionsButton = backFromInstructions;
        }

        // ----- helpers -----

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // A horizontal bar with a black background and a coloured fill image
        // anchored to the top-left of its parent.
        //
        // IMPORTANT: An Image with type=Filled but no Sprite renders as a full
        // quad and ignores fillAmount entirely (that's why the bars used to
        // never visibly change). We assign a tiny runtime-created white sprite
        // so the Filled mode actually has something to clip.
        static Image MakeBar(Transform parent, string name, Vector2 anchoredTopLeft, Color fillColor,
            float width = 280f, float height = 22f)
        {
            Sprite whiteSprite = GetWhiteSprite();

            GameObject root = new GameObject(name + "Bar",
                typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var bg = root.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            bg.sprite = whiteSprite;
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredTopLeft;
            rt.sizeDelta = new Vector2(width, height);

            GameObject fillGO = new GameObject(name + "Fill",
                typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(root.transform, false);
            var fill = fillGO.GetComponent<Image>();
            fill.sprite = whiteSprite;
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            var fr = fillGO.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(2, 2);
            fr.offsetMax = new Vector2(-2, -2);
            return fill;
        }

        static Sprite cachedWhiteSprite;

        static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite != null) return cachedWhiteSprite;
            cachedWhiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f));
            return cachedWhiteSprite;
        }

        static GameObject MakePanel(Transform parent, string name, Color bg)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var img = panel.GetComponent<Image>();
            img.color = bg;
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return panel;
        }

        static Text MakeText(
            Transform parent, string name, string content, Vector2 pos,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(700, 60);
            return text;
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 60);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.15f, 0.18f, 0.22f, 0.9f);

            Button btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.15f, 0.18f, 0.22f, 0.9f);
            cb.highlightedColor = new Color(0.3f, 0.45f, 0.55f, 0.95f);
            cb.pressedColor = new Color(0.5f, 0.7f, 0.9f, 1f);
            btn.colors = cb;

            GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGO.transform.SetParent(go.transform, false);
            var txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;
            Text t = txtGO.GetComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 26;
            t.color = new Color(0.95f, 0.95f, 0.95f);

            return btn;
        }
    }
}
