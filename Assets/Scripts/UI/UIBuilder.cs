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

            // ----- HUD panel -----
            GameObject hudPanel = MakePanel(canvas.transform, "HUD", new Color(0, 0, 0, 0));
            Text fragmentText = MakeText(hudPanel.transform, "FragmentText",
                "Memories: 0 / 5", new Vector2(40, -40),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 28, TextAnchor.UpperLeft);
            Text messageText = MakeText(hudPanel.transform, "MessageText",
                "", new Vector2(0, 220),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), 30, TextAnchor.MiddleCenter);
            messageText.color = new Color(1f, 0.95f, 0.7f);

            // Battery bar.
            GameObject batteryRoot = new GameObject("BatteryRoot",
                typeof(RectTransform), typeof(Image));
            batteryRoot.transform.SetParent(hudPanel.transform, false);
            var bbg = batteryRoot.GetComponent<Image>();
            bbg.color = new Color(0, 0, 0, 0.5f);
            var brt = batteryRoot.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 1);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(0, 1);
            brt.anchoredPosition = new Vector2(40, -90);
            brt.sizeDelta = new Vector2(280, 22);

            GameObject batteryFillGO = new GameObject("BatteryFill",
                typeof(RectTransform), typeof(Image));
            batteryFillGO.transform.SetParent(batteryRoot.transform, false);
            var fillImg = batteryFillGO.GetComponent<Image>();
            fillImg.color = new Color(1f, 0.85f, 0.5f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            var ffrt = batteryFillGO.GetComponent<RectTransform>();
            ffrt.anchorMin = Vector2.zero;
            ffrt.anchorMax = Vector2.one;
            ffrt.offsetMin = new Vector2(2, 2);
            ffrt.offsetMax = new Vector2(-2, -2);

            MakeText(hudPanel.transform, "BatteryLabel",
                "Flashlight (F)", new Vector2(40, -120),
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), 18, TextAnchor.UpperLeft).color
                = new Color(1, 1, 1, 0.6f);

            MakeText(hudPanel.transform, "Hint",
                "WASD: Move    F: Flashlight    ESC: Pause",
                new Vector2(0, 30),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                18, TextAnchor.MiddleCenter).color = new Color(1, 1, 1, 0.4f);

            HUDController hud = canvasGO.AddComponent<HUDController>();
            hud.fragmentText = fragmentText;
            hud.messageText = messageText;
            hud.batteryFill = fillImg;
            if (player != null)
                hud.Bind(player.GetComponent<PlayerStats>(), player.GetComponent<PlayerFlashlight>());

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
            Button quitMain = MakeButton(mainMenu.transform, "QuitButton", "Quit", new Vector2(0, -120));

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
        }

        // ----- helpers -----

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
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
