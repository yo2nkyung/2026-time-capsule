using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;



public class GoalkeeperMode : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] soccerBalls;     // Soccer balls
    public float ballSpeed = 5f;         // ball speed
    public float blockRange = 1.5f;      // block range
    public float spawnInterval = 3f;     // ball spawn interval


    [Header("Goal Target")]
    public Transform goalCenter;


    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;   // "SAVED!" / "GOAL!"
    
    
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip saveSound;
    public AudioClip goalSound;


    private int savedCount = 0;
    private int goalCount = 0;
    private bool ballInFlight = false;
    private GameObject currentBall;
    private Vector3[] ballStartPositions;


    void OnEnable()
    {
        // resetting everthing
        savedCount = 0;
        goalCount = 0;
        UpdateScoreUI();
        resultText.text = "";
        ballInFlight = false;

        // initial location of ball
        ballStartPositions = new Vector3[soccerBalls.Length];
        for (int i = 0; i < soccerBalls.Length; i++)
            {
                ballStartPositions[i] = soccerBalls[i].transform.position;
            }
        StartCoroutine(LaunchRoutine());


    }

    void OnDisable()
    {
        StopAllCoroutines();
        ResetAllBalls();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!ballInFlight || currentBall == null) return;

        // move ball to goalkeeper direction
        currentBall.transform.position = Vector3.MoveTowards(
            currentBall.transform.position,
            Camera.main.transform.position,
            ballSpeed * Time.deltaTime);

        float dist = Vector3.Distance(
            currentBall.transform.position,
            Camera.main.transform.position
        );

        if (dist < blockRange)
        {
            Vector3 dirToBall = (currentBall.transform.position - Camera.main.transform.position).normalized;
            float angle = Vector3.Angle(Camera.main.transform.forward, dirToBall);

            if (angle < 60f) OnSave();
            else OnGoal();
        }

    }

    IEnumerator LaunchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (!ballInFlight) LaunchRandomBall();
        }
    }

    void LaunchRandomBall()
    {
        int idx = Random.Range(0, soccerBalls.Length);
        currentBall = soccerBalls[idx];

        currentBall.transform.position = goalCenter.position + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), 0);

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // move with 'movetowards' in Update()

        ballInFlight = true;
        Debug.Log($"[GoalkeeperMode] Launched: {currentBall.name}");

    }

    void OnSave()
    {

        savedCount++;
        UpdateScoreUI();
        resultText.text = "SAVED!";
        if (saveSound != null) audioSource.PlayOneShot(saveSound);
        ballInFlight = false;
    }

    void OnGoal()
    {
        goalCount++;
        UpdateScoreUI();
        resultText.text = "GOAL!";
        if (goalSound != null) audioSource.PlayOneShot(goalSound);
        ballInFlight = false;

        ResetBall(currentBall);
        currentBall = null;
    }

    void ShowResult(string msg, bool saved)
    {
        resultText.text = msg;
        StartCoroutine(ClearResult());
    }

    IEnumerator ClearResult()
    {
        yield return new WaitForSeconds(1.5f);
        resultText.text = "";
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SAVED: {savedCount}  |  GOALS: {goalCount}";
    }

    void ResetBall(GameObject ball)
    {
        for (int i = 0; i < soccerBalls.Length; i++)
        {
            if (soccerBalls[i] == ball)
            {
                ball.transform.position = ballStartPositions[i];
                break;
            }
        }
    }

    void ResetAllBalls()
    {
        for (int i = 0; i < soccerBalls.Length; i++)
        {
            soccerBalls[i].transform.position = ballStartPositions[i];
        }
    }

}
