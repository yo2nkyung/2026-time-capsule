using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    private const string GoalMessage = "GOAL!!!!";
    private const float DisplayDuration = 3f;

    private void Start()
    {
        var col = GetComponent<Collider>();
        if (col == null || !col.isTrigger)
            Debug.LogError($"[GoalDetector] {name}: No trigger Collider found. Attach a Collider with isTrigger=true.", this);

        if (GetComponent<Rigidbody>() == null)
            Debug.LogWarning($"[GoalDetector] {name}: No Rigidbody on this trigger. Add a kinematic Rigidbody to guarantee OnTriggerEnter fires.", this);

        Debug.Log($"[GoalDetector] {name}: Initialized. Bounds center={col?.bounds.center}, size={col?.bounds.size}", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GoalDetector] {name}: OnTriggerEnter hit by '{other.name}' (rb={other.attachedRigidbody != null})", this);

        if (other.attachedRigidbody == null)
        {
            Debug.Log($"[GoalDetector] Ignored '{other.name}' — no attached Rigidbody.", this);
            return;
        }

        if (HUDMessageController.Instance != null)
        {
            HUDMessageController.Instance.ShowMessage(GoalMessage, DisplayDuration);
        }
        else
        {
            Debug.LogWarning("[GoalDetector] HUDMessageController.Instance is null — GOAL triggered but cannot display message. Ensure HUDCanvas exists in the scene or is carried from ARScene.", this);
        }
    }
}
