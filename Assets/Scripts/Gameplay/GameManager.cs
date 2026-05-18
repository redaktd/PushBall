using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int levelId = 1;
    [SerializeField] private float winTimerDelay = 2f;

    [Header("Object References")]
    [SerializeField] private GameObject playerBall;
    [SerializeField] private GameObject pushBox;
    [SerializeField] private EndScreen endScreen;
    [SerializeField] private TMP_Text timeText;

    private bool _isResetting;

    // Tracked metrics
    public float TimeSeconds { get; private set; }
    public int Deaths { get; private set; }
    public int Score { get; private set; }

    // Stored start positions for reset
    private Vector3 _ballStartPos;
    private Vector3 _boxStartPos;

    private bool _levelComplete;
    private bool _timerRunning;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        Time.timeScale = 1;

        // Store starting positions for death reset
        _ballStartPos = playerBall.transform.position;
        _boxStartPos = pushBox.transform.position;

        // Start the timer
        _timerRunning = true;
    }

    private void Update()
    {
        if (_timerRunning)
        {
            TimeSeconds += Time.deltaTime;
            timeText.text = FormatTime(TimeSeconds);
        }
    }

    // ---------------------------------------------------------------
    // Called by StateController when box reaches WinZone
    // ---------------------------------------------------------------
    public void LevelComplete()
    {
        if (_levelComplete) return; // prevent double trigger
        _levelComplete = true;
        _timerRunning = false;

        Score = CalculateScore();

        StartCoroutine(ShowEndScreen());
    }

    private IEnumerator ShowEndScreen()
    {
        yield return new WaitForSeconds(winTimerDelay);
        Time.timeScale = 0;
        endScreen.Show(levelId, TimeSeconds, Deaths, Score);
    }

    // ---------------------------------------------------------------
    // Called by OutOfBounds and KillSurface on death
    // ---------------------------------------------------------------
    public void RegisterDeath()
    {
        if (_levelComplete) return; // ignore deaths after winning
        if (_isResetting) return;  // blocks double triggers

        Deaths++;
        StartCoroutine(ResetLevel());
    }

    private IEnumerator ResetLevel()
    {
        _isResetting = true;

        yield return new WaitForSeconds(0.5f);

        // Reset positions
        playerBall.transform.position = _ballStartPos;
        pushBox.transform.position = _boxStartPos;

        // Reset rigidbodies so they don't carry momentum
        ResetRigidbody(playerBall);
        ResetRigidbody(pushBox);

        _isResetting = false;
    }

    private void ResetRigidbody(GameObject obj)
    {
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // ---------------------------------------------------------------
    // Score formula — rewards speed and clean runs
    // ---------------------------------------------------------------
    private int CalculateScore()
    {
        int baseScore = 10000;
        int timePenalty = Mathf.RoundToInt(TimeSeconds * 10);
        int deathPenalty = Deaths * 500;
        int finalScore = Mathf.Max(0, baseScore - timePenalty - deathPenalty);
        return finalScore;
    }

    // ---------------------------------------------------------------
    // Format Time — convert time from seconds to an easily readable format
    // ---------------------------------------------------------------
    public string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        int ms = (int)((seconds * 1000) % 1000);
        return $"{m:00}:{s:00}:{ms:000}";
    }

    public int GetLevelId()
    {
        return levelId;
    }
}