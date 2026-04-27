using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public BallPlayerController2D player;
    public PlayerBallAudio playerBallAudio;
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject controlsPanel;
    public GameObject statusPanel;
    public GameObject pausePanel;
    public GameObject winPanel;
    
    [Header("UI Text")]
    public TextMeshProUGUI statusText;

    private bool hasWon;
    private bool gameStarted;
    private bool isPaused;

    private float currentRunTime;
    private float bestTime = -1f;

    private const string BestTimeKey = "BestTime";

    private void Start()
    {
        Time.timeScale = 0f;
        gameStarted = false;
        isPaused = false;
        hasWon = false;
        currentRunTime = 0f;
        if (PlayerPrefs.HasKey(BestTimeKey))
        {
            bestTime = PlayerPrefs.GetFloat(BestTimeKey);
        }

        if (startPanel != null) startPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (statusPanel != null) statusPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        UpdateStatusUI();
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        if (!hasWon && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!isPaused && !hasWon)
        {
            currentRunTime += Time.deltaTime;
        }

        UpdateStatusUI();
    }

    public void BeginGame()
    {
        gameStarted = true;
        isPaused = false;
        hasWon = false;
        currentRunTime = 0f;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (startPanel != null) startPanel.SetActive(false);
        if (statusPanel != null) statusPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        if (player != null)
        {
            player.RespawnAtBottom();
            player.LockInputForSeconds(0.2f);
        }

        UpdateStatusUI();
    }

    public void PlayAgain()
    {
        gameStarted = true;
        isPaused = false;
        hasWon = false;
        currentRunTime = 0f;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (startPanel != null) startPanel.SetActive(false);
        if (statusPanel != null) statusPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (player != null)
        {
            player.RespawnAtBottom();
            player.LockInputForSeconds(0.2f);
        }

        UpdateStatusUI();
    }

    public void HideControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        if (statusPanel != null)
            statusPanel.SetActive(!isPaused);
    }


    public void ReturnToStartMenu()
    {
        gameStarted = false;
        isPaused = false;
        hasWon = false;
        currentRunTime = 0f;
        Time.timeScale = 0f;

        if (startPanel != null) startPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (statusPanel != null) statusPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        AudioListener.pause = true;
        if (player != null)
        {
            player.RespawnAtBottom();
            player.LockInputForSeconds(0.1f);
        }

        UpdateStatusUI();
    }


    public void SetWinState(bool won)
    {
        if (!won || hasWon)
            return;

        hasWon = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (bestTime < 0f || currentRunTime < bestTime)
        {
            bestTime = currentRunTime;
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            PlayerPrefs.Save();
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        UpdateStatusUI();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateStatusUI()
    {
        if (statusText == null || player == null)
            return;

        string bestTimeDisplay = bestTime >= 0f ? FormatTime(bestTime) : "--:--.--";

        statusText.text =
            $"Height: {player.transform.position.y + 3f:0.0}\n" +
            $"Grounded: {player.IsGrounded}\n" +
            $"Hooked: {player.IsHooked}\n" +
            $"Time: {FormatTime(currentRunTime)}\n" +
            $"Best: {bestTimeDisplay}";
    }

    private string FormatTime(float timeSeconds)
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        float seconds = timeSeconds % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }
}