using UnityEngine;
using System.Collections;
using TMPro;

public class SuperbowlGameManager : MonoBehaviour
{
    public static SuperbowlGameManager Instance;

    [Header("Game")]
    public float gameTime = 30f;
    public int targetScore = 5;

    [Header("Footballs")]
    public Rigidbody[] footballs;

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public GameObject resultPanel;
    public TMP_Text resultText;

    public TMP_Text pointPopupText;

    private int thrownCount = 0;
    private Coroutine pointPopupCoroutine;
    private Coroutine runOutCoroutine;

    

    private float timer;
    private int score;
    private bool isPlaying;
    public GameObject startPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        originalPositions = new Vector3[footballs.Length];
        originalRotations = new Quaternion[footballs.Length];

        for (int i = 0; i < footballs.Length; i++)
        {
            originalPositions[i] = footballs[i].transform.position;
            originalRotations[i] = footballs[i].transform.rotation;
        }


        isPlaying = false;
        timer = gameTime;
        score = 0;

        startPanel.SetActive(true);
        resultPanel.SetActive(false);

    UpdateUI();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        UpdateUI();

        if (timer <= 0)
        {
            EndGame("FAILED!\nTime is up!\nScore: " + score);
        }
    }

    public void StartGame()
    {
        if (runOutCoroutine != null)
        {
            StopCoroutine(runOutCoroutine);
            runOutCoroutine = null;
        }
        ResetFootballs();
        timer = gameTime;
        score = 0;
        isPlaying = true;

        thrownCount = 0;

        if (pointPopupText != null)
            pointPopupText.text = "";

        startPanel.SetActive(false);
        resultPanel.SetActive(false);

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (!isPlaying) return;

        score += amount;
        UpdateUI();
        ShowPointPopup(amount);
        if (score >= targetScore)
        {
            EndGame("SUCCESS!\nScore: " + score);
        }
    }

    void EndGame(string message)
    {
        isPlaying = false;

        if (resultPanel != null) resultPanel.SetActive(true);

        resultText.text = message;
    }

    void UpdateUI()
    {
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();
        scoreText.text = "Score: " + score + " / " + targetScore;
    }

    public void RestartGame()
    {
        StartGame();
    }

    void ResetFootballs()
    {
        for (int i = 0; i < footballs.Length; i++)
        {
            footballs[i].linearVelocity = Vector3.zero;
            footballs[i].angularVelocity = Vector3.zero;

            footballs[i].transform.position = originalPositions[i];
            footballs[i].transform.rotation = originalRotations[i];

            ScoredBall scoredBall = footballs[i].GetComponent<ScoredBall>();
            if (scoredBall != null)
                scoredBall.hasScored = false;
        }
    }

    public void RegisterBallThrown()
    {
        if (!isPlaying) return;

        thrownCount++;

        if (thrownCount >= footballs.Length && score < targetScore)
        {
            if (runOutCoroutine != null)
                StopCoroutine(runOutCoroutine);

            runOutCoroutine = StartCoroutine(CheckRunOutOfBalls());
        }
    }

    IEnumerator CheckRunOutOfBalls()
    {
        yield return new WaitForSeconds(2.5f);

        if (isPlaying && score < targetScore)
        {
            EndGame("FAILED!\nRun out of balls!\nScore: " + score);
        }
        runOutCoroutine = null;
    }

    void ShowPointPopup(int amount)
    {
        if (pointPopupText == null) return;

        if (pointPopupCoroutine != null)
            StopCoroutine(pointPopupCoroutine);

        pointPopupCoroutine = StartCoroutine(PointPopupRoutine(amount));
    }

    IEnumerator PointPopupRoutine(int amount)
    {
        pointPopupText.text = "+" + amount + " POINTS!";

        yield return new WaitForSeconds(1.2f);

        pointPopupText.text = "";
    }
}