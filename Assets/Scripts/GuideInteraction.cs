using UnityEngine;
using TMPro;

public class GuideInteraction : MonoBehaviour
{
    public float maxDistance = 8f;

    public GameObject promptObject;
    public GameObject dialoguePanel;

    public TMP_Text titleText;
    public TMP_Text answerText;

    private GuideNPC currentGuide;
    private GuideNPC activeGuide;

    void Start()
    {
        promptObject.SetActive(false);
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf)
        {
            CheckForGuide();

            if (currentGuide != null)
            {
                promptObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E) ||
                    Input.GetMouseButtonDown(0) ||
                    Input.GetKeyDown(KeyCode.JoystickButton10))
                {
                    OpenDialogue();
                }
            }
            else
            {
                promptObject.SetActive(false);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                CloseDialogue();
            }
        }
    }

    void CheckForGuide()
    {
        RaycastHit hit;
        currentGuide = null;

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            currentGuide = hit.collider.GetComponentInParent<GuideNPC>();
        }
    }

    void OpenDialogue()
    {
        activeGuide = currentGuide;

        dialoguePanel.SetActive(true);
        promptObject.SetActive(false);

        titleText.text = activeGuide.gameObject.name;
        answerText.text = activeGuide.intro;
    }

    public void ShowEventInfo()
    {
        if (activeGuide != null)
        {
            answerText.text = activeGuide.eventInfo;
        }
    }

    public void ShowInstructions()
    {
        if (activeGuide != null)
        {
            answerText.text = activeGuide.instructions;
        }
    }

    public void ShowNextStep()
    {
        if (activeGuide != null)
        {
            answerText.text = activeGuide.nextStep;
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        promptObject.SetActive(false);
    }
}