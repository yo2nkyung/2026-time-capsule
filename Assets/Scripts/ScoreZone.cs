using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    public int scoreValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Superbowl"))
            return;

        ScoredBall scoredBall = other.GetComponent<ScoredBall>();

        if (scoredBall == null)
            scoredBall = other.gameObject.AddComponent<ScoredBall>();

        if (scoredBall.hasScored)
            return;

        scoredBall.hasScored = true;

        SuperbowlGameManager.Instance.AddScore(scoreValue);
    }
}