using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.FinishGame(true);
        }
    }
}