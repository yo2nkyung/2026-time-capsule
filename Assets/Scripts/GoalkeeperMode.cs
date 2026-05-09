using System.Collections;
using UnityEngine;

public class GoalkeeperMode : MonoBehaviour
{
    [Header("XR")]
    public Transform xrOrigin;
    public Transform goalkeeperSpawnPoint;

    [Header("Ball")]
    public GameObject ballPrefab;

    public Transform[] ballSpawnPoints;

    public Transform[] targetPoints;

    public float shootInterval = 4f;

    private bool isPlaying = false;
    private Coroutine shootRoutine;

    public int maxShots = 5;
    public int saveCount = 0;

    public TMPro.TMP_Text saveCountText;
    public TMPro.TMP_Text messageText;
    public GameObject goalkeeperHUD;

private int shotsTaken = 0;


    void OnEnable()
    {
        StartGoalkeeperMode();
    }

    void OnDisable()
    {
        isPlaying = false;
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        if (goalkeeperHUD != null) goalkeeperHUD.SetActive(false);
    }

    public void StartGoalkeeperMode()
    {
        xrOrigin.position = goalkeeperSpawnPoint.position;
        xrOrigin.rotation = goalkeeperSpawnPoint.rotation;

        saveCount = 0;
        shotsTaken = 0;
        goalkeeperHUD.SetActive(true);
        UpdateSaveText();
        messageText.text = "";

        isPlaying = true;
        shootRoutine = StartCoroutine(ShootBallsRoutine());
    }

    IEnumerator ShootBallsRoutine()
    {
        while (isPlaying && shotsTaken < maxShots)
        {   
            shotsTaken++;
            Transform spawn = ballSpawnPoints[Random.Range(0, ballSpawnPoints.Length)];
            Transform target = targetPoints[Random.Range(0, targetPoints.Length)];
            GameObject ball = Instantiate(ballPrefab, spawn.position, Quaternion.identity);

            BallBlockDetector detector = ball.GetComponent<BallBlockDetector>();
            detector.goalkeeperMode = this;

            Rigidbody rb = ball.GetComponent<Rigidbody>();

            Vector3 direction = (target.position - spawn.position).normalized;
            float speed = 16f;

            rb.linearVelocity = direction * speed;
            rb.angularVelocity = Vector3.zero;  

            Destroy(ball, 8f);

            yield return new WaitForSeconds(shootInterval);
        }
        isPlaying = false;
        messageText.text = "Done! Saves: " + saveCount + " / " + maxShots;
    }

    public void RegisterSave()
    {
        saveCount++;
        UpdateSaveText();

        messageText.text = "SAVE!";
    }

    public void RegisterMiss()
    {
        messageText.text = "MISS!";
    }

    void UpdateSaveText()
    {
        saveCountText.text = "Saves: " + saveCount + " / " + maxShots;
    }
}