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

    [Header("OpenAI")]
    [TextArea(2, 4)]
    public string apiKey = "";
    public string modelName = "gpt-5.4-nano";
    public bool useFakeResponse = true;

    [Header("Assistant Instructions")]
    [TextArea(3, 6)]
    public string assistantInstructions =
    "You are a friendly conversational avatar inside a student Unity AR time capsule project. " +
    "Keep replies short, natural, and helpful. " +
    "Use plain text only. Do not use markdown, bold, bullet points, or asterisks. " +
    "Keep each reply to 2 to 4 short sentences max. " +
    "Use the current scene context when relevant.";

    private const string ApiUrl = "https://api.openai.com/v1/responses";

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendClicked);

        if (responseText != null)
            responseText.text = "Hi! Ask me something about this scene.";
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
                return "Main AR scene where the user selects portals and interacts with the time capsule.";
            case "soccer":
                return "Soccer-related scene in the time capsule experience.";
            case "timecapsule_room":
                return "A room scene for viewing time capsule content.";
            case "EndScene":
                return "The ending scene of the experience.";
            default:
                return "This is a Unity AR scene in the project.";
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
