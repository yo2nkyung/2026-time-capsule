using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    StartScreen,
    Countdown,
    Playing,
    Finished
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState CurrentState = GameState.StartScreen;

    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject finishPanel;

    public TMP_Text countdownText;
    public TMP_Text resultText;
    public TMP_Text timerText;

    public float gameDuration = 30f;

    private float timer;

    public Transform trackRoot;
    private Vector3 originalTrackPosition;

    void Awake()
    {
        Instance = this;
        originalTrackPosition = trackRoot.position;

        CurrentState = GameState.StartScreen;

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);

        countdownText.gameObject.SetActive(false);
    }

    void Start()
    {
        CurrentState = GameState.StartScreen;

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);

        countdownText.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        CurrentState = GameState.Countdown;

        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        finishPanel.SetActive(false);

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.7f);

        countdownText.gameObject.SetActive(false);

        timer = gameDuration;
        timerText.text = "Time: " + gameDuration.ToString();
        CurrentState = GameState.Playing;
    }

    void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        timer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

        if (timer <= 0)
        {
            FinishGame(false);
        }
    }

    public void FinishGame(bool success)
    {
        CurrentState = GameState.Finished;

    gamePanel.SetActive(false);
    finishPanel.SetActive(true);

    if (success)
        resultText.text = "SUCCESS!\nYou reached the finish line!";
    else
        resultText.text = "FAILED!\nTime is up!";
    }

    public void RestartGame()
    {
        trackRoot.position = originalTrackPosition;

    
        StartGame();
    }
}