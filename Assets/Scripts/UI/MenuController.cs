using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ZombieLand.Managers;

namespace ZombieLand.UI
{
    /// <summary>
    /// Switches between the three full-screen UI panels (Main / Pause / Win)
    /// based on GameManager state, and wires the buttons.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        public GameObject mainMenuPanel;
        public GameObject pauseMenuPanel;
        public GameObject winPanel;
        public GameObject lostPanel;
        public GameObject hudPanel;

        public Button startButton;
        public Button quitFromMainButton;
        public Button resumeButton;
        public Button restartFromPauseButton;
        public Button quitFromPauseButton;
        public Button restartFromWinButton;
        public Button quitFromWinButton;
        public Button retryFromLostButton;
        public Button quitFromLostButton;

        public Text winSummaryText;

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            BindButton(startButton, () => GameManager.Instance?.StartGame());
            BindButton(resumeButton, () => GameManager.Instance?.TogglePause());
            BindButton(restartFromPauseButton, () => GameManager.Instance?.Restart());
            BindButton(restartFromWinButton, () => GameManager.Instance?.Restart());
            BindButton(quitFromMainButton, () => GameManager.Instance?.Quit());
            BindButton(quitFromPauseButton, () => GameManager.Instance?.Quit());
            BindButton(quitFromWinButton, () => GameManager.Instance?.Quit());
            BindButton(retryFromLostButton, () => GameManager.Instance?.Restart());
            BindButton(quitFromLostButton, () => GameManager.Instance?.Quit());

            OnStateChanged(GameManager.Instance != null ? GameManager.Instance.CurrentState : GameManager.State.Menu);
        }

        void OnStateChanged(GameManager.State state)
        {
            if (mainMenuPanel)  mainMenuPanel.SetActive(state == GameManager.State.Menu);
            if (pauseMenuPanel) pauseMenuPanel.SetActive(state == GameManager.State.Paused);
            if (winPanel)       winPanel.SetActive(state == GameManager.State.Won);
            if (lostPanel)      lostPanel.SetActive(state == GameManager.State.Lost);
            if (hudPanel)       hudPanel.SetActive(state == GameManager.State.Playing || state == GameManager.State.Paused);

            if (state == GameManager.State.Won) FillWinSummary();
        }

        void FillWinSummary()
        {
            if (winSummaryText == null || GameManager.Instance == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("You remembered everything.");
            sb.AppendLine();
            foreach (string memory in GameManager.Instance.rememberedMemories)
                sb.AppendLine("• " + memory);
            winSummaryText.text = sb.ToString();
        }

        static void BindButton(Button button, System.Action onClick)
        {
            if (button == null || onClick == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }
}
