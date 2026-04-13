using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Time capsule controller for the ATM object.
public class TimeCapsuleController : MonoBehaviour
{
    [Tooltip("The UI Text component inside FeedbackCanvas used to display messages.")]
    public Text feedbackText;

    [Tooltip("The world-space Canvas that holds the feedback text (used to billboard toward the camera).")]
    public Canvas feedbackCanvas;

    [Tooltip("How long (seconds) the feedback message is visible before fading.")]
    public float messageDuration = 2.5f;

    [Tooltip("Scale added to the ATM per collected item.")]
    public Vector3 scaleIncrement = new Vector3(0.15f, 0.15f, 0.15f);

    [Tooltip("Exact name of the end scene to load when the capsule is full.")]
    public string endSceneName = "EndScene";

    [Tooltip("Number of items required to fill the time capsule.")]
    public int requiredItemCount = 3;

    private const string NeedsMoreMemoriesMessage = "time capsule needs more memories";
    private const string ItemCollectedFormat = "{0} has been put in the time capsule";

    private readonly HashSet<string> _collectedThisSession = new HashSet<string>();
    private Coroutine _hideMessageCoroutine;

    private void Awake()
    {
        TimeCapsuleGameData.Reset();
    }

    private void LateUpdate()
    {
        if (feedbackCanvas != null && feedbackCanvas.gameObject.activeSelf)
        {
            Transform cam = Camera.main != null ? Camera.main.transform : null;
            if (cam != null)
                feedbackCanvas.transform.LookAt(feedbackCanvas.transform.position + cam.forward);
        }
    }

    // called when the ATM is tapped
    public void OnTapped()
    {
        if (TimeCapsuleGameData.CollectedItems.Count >= requiredItemCount)
        {
            SceneManager.LoadScene(endSceneName);
            return;
        }

        ShowMessage(NeedsMoreMemoriesMessage);
        HUDMessageController.Instance?.ShowMessage(NeedsMoreMemoriesMessage, messageDuration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        BallThrowable ball = collision.gameObject.GetComponent<BallThrowable>();
        if (ball == null)
            return;

        string itemName = collision.gameObject.name;
        if (_collectedThisSession.Contains(itemName))
            return;

        _collectedThisSession.Add(itemName);
        TimeCapsuleGameData.CollectedItems.Add(itemName);

        string collectedMessage = string.Format(ItemCollectedFormat, itemName);
        ShowMessage(collectedMessage);
        HUDMessageController.Instance?.ShowMessage(collectedMessage, messageDuration);
        GrowATM();
        collision.gameObject.SetActive(false);
    }

    private void ShowMessage(string message)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        if (_hideMessageCoroutine != null)
            StopCoroutine(_hideMessageCoroutine);

        _hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(messageDuration));
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(false);
        _hideMessageCoroutine = null;
    }

    private void GrowATM()
    {
        transform.localScale += scaleIncrement;
    }
}
