using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// AR chat assistant - sends user input to OpenAI and displays response
public class ARChatAssistant : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text responseText;
    public Button sendButton;
    public GameObject chatPanel;

    [Header("OpenAI")]
    [TextArea(2, 4)]
    public string apiKey = "";
    public string modelName = "gpt-5.4-mini";
    public bool useFakeResponse = true;

    [Header("Assistant Instructions")]
    [TextArea(3, 6)]
    public string assistantInstructions =
    "You are a friendly conversational guide inside an AR project called Time Capsule. " +
    "Help the user understand what they are seeing, what they can do in the current scene, and what to do next. " +
    "Respond in plain text only. Keep replies short and natural. " +
    "Most replies should be 1 to 3 short sentences. " +
    "Do not use markdown, bullet points, or asterisks. " +
    "Be scene-aware, specific, and easy to follow like an in-world host.";
    private const string ApiUrl = "https://api.openai.com/v1/responses";

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);

        if (responseText != null)
            responseText.text = "Hi! Ask me something about this scene.";
    }

    public void ToggleChatPanel()
    {
        if (chatPanel == null)
            return;

        chatPanel.SetActive(!chatPanel.activeSelf);
    }

    public void OnSendClicked()
    {
        if (inputField == null || responseText == null || sendButton == null)
            return;

        string userMessage = inputField.text.Trim();

        if (string.IsNullOrEmpty(userMessage))
            return;

        sendButton.interactable = false;
        responseText.text = "Thinking...";

        if (useFakeResponse)
            StartCoroutine(FakeReply(userMessage));
        else
            StartCoroutine(SendToOpenAI(userMessage));
    }

    IEnumerator FakeReply(string userMessage)
    {
        yield return new WaitForSeconds(0.5f);

        string sceneName = SceneManager.GetActiveScene().name;
        string sceneDescription = GetSceneDescription(sceneName);

        responseText.text =
            "Scene: " + sceneName + "\n" +
            "Info: " + sceneDescription + "\n\n" +
            "You asked: " + userMessage + "\n\n" +
            "This is a fake test reply.";

        inputField.text = "";
        sendButton.interactable = true;
    }

    IEnumerator SendToOpenAI(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            responseText.text = "Missing API key in Inspector.";
            sendButton.interactable = true;
            yield break;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        string sceneDescription = GetSceneDescription(sceneName);
        string prompt = BuildPrompt(userMessage, sceneName, sceneDescription);

        OpenAIRequest requestBody = new OpenAIRequest
        {
            model = modelName,
            instructions = assistantInstructions,
            input = prompt
        };

        string json = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                responseText.text = "Request failed:\n" + request.error + "\n\n" + request.downloadHandler.text;
            }
            else
            {
                string rawJson = request.downloadHandler.text;
                OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(rawJson);
                string reply = ExtractReplyText(response);

                if (string.IsNullOrEmpty(reply))
                    responseText.text = "No text found in response.\n\n" + rawJson;
                else
                {
                    responseText.text = reply;
                    inputField.text = "";
                }
            }
        }

        sendButton.interactable = true;
    }

    string BuildPrompt(string userMessage, string sceneName, string sceneDescription)
    {
        return
            "Current Unity scene: " + sceneName + "\n" +
            "Scene description: " + sceneDescription + "\n" +
            "User message: " + userMessage + "\n\n" +
            "Respond as a helpful conversational avatar inside the AR experience. " +
            "Keep the answer concise and scene-aware.";
    }

    string GetSceneDescription(string sceneName)
    {
        switch (sceneName)
        {
            case "ARScene":
                return "This is the main AR entry scene. The user places the Time Capsule hub into their real space, then sees the Time Capsule and 2026 Shelf appear. From here, the user can either enter the timecapsule_room to choose a minigame scene or interact with shelf items to fill the Time Capsule, which grows as items are added. The assistant should help the user get oriented and explain these two paths clearly.";

            case "timecapsule_room":
                return "This is the main Time Capsule scene-selection hub. The user chooses which 2026 experience or minigame to enter next by selecting one of the available scene options. The assistant should act like a welcoming guide, explain what each choice leads to, and help the user decide where to go next.";

            case "soccer":
                return "This is the 2026 World Cup themed scene. The main interaction is moving the ball into the goal, so the user should feel like they are stepping into a soccer event moment from 2026. The assistant should sound energetic and encouraging while clearly explaining the goal of the minigame and what the user should do.";

            case "olympic":
                return "This is the 2026 Olympics themed scene. It is a bobsleigh-style experience where the user mainly interacts by tilting their head to control or follow the ride. The assistant should be clear and supportive, helping the user understand the motion-based interaction and what the scene represents.";

            case "EndScene":
                return "This is the ending scene of the Time Capsule experience. It shows that the user's Time Capsule is complete and summarizes the items they collected or saw during the experience. The assistant should sound celebratory and reflective while helping the user understand that they have completed the journey.";

            default:
                return "This is a scene in the Time Capsule AR project. The assistant should help the user understand what they are seeing, what they can do here, and how this scene fits into the overall experience.";
        }
    }

    string ExtractReplyText(OpenAIResponse response)
    {
        if (response == null)
            return "";

        if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            return "API error: " + response.error.message;

        if (response.output == null)
            return "";

        for (int i = 0; i < response.output.Length; i++)
        {
            OutputItem item = response.output[i];
            if (item == null || item.content == null)
                continue;

            for (int j = 0; j < item.content.Length; j++)
            {
                ContentItem content = item.content[j];
                if (content != null && !string.IsNullOrEmpty(content.text))
                    return content.text;
            }
        }

        return "";
    }

    [System.Serializable]
    public class OpenAIRequest
    {
        public string model;
        public string instructions;
        public string input;
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public OutputItem[] output;
        public ErrorItem error;
    }

    [System.Serializable]
    public class OutputItem
    {
        public ContentItem[] content;
    }

    [System.Serializable]
    public class ContentItem
    {
        public string type;
        public string text;
    }

    [System.Serializable]
    public class ErrorItem
    {
        public string message;
    }
}
