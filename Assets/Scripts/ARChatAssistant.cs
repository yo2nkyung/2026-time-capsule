using System.Collections;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ARChatAssistant : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public TextMeshProUGUI responseText;
    public Button sendButton;
    public GameObject chatPanel;

    [Header("OpenAI")]
    public string apiKey = "";
    public string modelName = "gpt-5.4-mini";
    public bool useFakeResponse = true;

    [Header("Voice")]
    public bool useTextToSpeech = true;
    public string ttsModel = "gpt-4o-mini-tts";
    public string ttsVoice = "coral";
    public AudioSource audioSource;

    [TextArea(4, 8)]
    public string assistantInstructions =
        "You are a friendly conversational guide inside an AR project called Time Capsule. " +
        "Act like an in-world host who helps the user understand the current scene, what the goal is, and what to do next. " +
        "Always give clear minigame instructions when the user is in a playable scene. " +
        "Mention the event name when relevant, like the 2026 Olympics, 2026 World Cup, or 2026 Super Bowl. " +
        "If the user is in the AR entry scene or the Time Capsule hub, explain their available paths clearly, including choosing a portal or adding collected memorabilia to the 2026 shelf. " +
        "Respond in plain text only. Keep replies short and natural. " +
        "Most replies should be 1 to 3 short sentences. " +
        "Do not use markdown, bullet points, or asterisks. " +
        "Be scene-aware, specific, encouraging, and easy to follow.";

    private const string ResponsesApiUrl = "https://api.openai.com/v1/responses";
    private const string TtsApiUrl = "https://api.openai.com/v1/audio/speech";

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }

        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }

        if (responseText != null)
        {
            responseText.text = "Hi! Ask me something about this scene.";
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void ToggleChatPanel()
    {
        if (chatPanel == null) return;

        bool newState = !chatPanel.activeSelf;
        chatPanel.SetActive(newState);

        if (newState && responseText != null && string.IsNullOrWhiteSpace(responseText.text))
        {
            responseText.text = "Hi! Ask me something about this scene.";
        }
    }

    public void OnSendClicked()
    {
        if (inputField == null) return;

        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        if (responseText != null)
        {
            responseText.text = "Thinking...";
        }

        if (useFakeResponse)
        {
            HandleFakeResponse(userMessage);
        }
        else
        {
            StartCoroutine(SendToOpenAI(userMessage));
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void HandleFakeResponse(string userMessage)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string sceneDescription = GetSceneDescription(sceneName);

        if (responseText != null)
        {
            responseText.text =
                "Scene: " + sceneName + "\n" +
                "Info: " + sceneDescription + "\n\n" +
                "You asked: " + userMessage + "\n" +
                "This is a fake test reply.";
        }
    }

    private IEnumerator SendToOpenAI(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (responseText != null)
            {
                responseText.text = "Missing API key in Inspector.";
            }
            yield break;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        string sceneDescription = GetSceneDescription(sceneName);
        string prompt = BuildPrompt(userMessage, sceneName, sceneDescription);

        OpenAIRequest requestBody = new OpenAIRequest
        {
            model = modelName,
            input = prompt
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(ResponsesApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (responseText != null)
                {
                    responseText.text = "Error: " + request.error + "\n" + request.downloadHandler.text;
                }
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            string parsedText = ExtractOutputText(jsonResponse);

            if (responseText != null)
            {
                responseText.text = string.IsNullOrEmpty(parsedText)
                    ? "No response received."
                    : parsedText;
            }

            if (useTextToSpeech && !string.IsNullOrEmpty(parsedText))
            {
                yield return StartCoroutine(PlayTextToSpeech(parsedText));
            }
        }
    }

    private string BuildPrompt(string userMessage, string sceneName, string sceneDescription)
    {
        return
            assistantInstructions + "\n\n" +
            "Current Unity scene: " + sceneName + "\n" +
            "Scene description: " + sceneDescription + "\n" +
            "User message: " + userMessage + "\n\n" +
            "Respond as a helpful conversational avatar inside the AR experience. " +
            "Use the scene description to explain the goal, controls, and next step. " +
            "Keep the answer concise, natural, and scene-aware.";
    }

    private string GetSceneDescription(string sceneName)
    {
        switch (sceneName)
        {
            case "ARScene":
                return "This is the main AR entry scene for Time Capsule. The user places the Time Capsule hub into their real space and starts the experience here. From this scene, the user has two main paths: they can select a portal that leads into the Time Capsule hub to choose a minigame, or they can interact with the 2026 shelf and add memorabilia objects they collected throughout the experience. The assistant should clearly explain these two options and help the user understand what to do next.";

            case "timecapsule_room":
                return "This is the main Time Capsule hub where the user chooses which 2026 event to explore next. The user can look around the room and select one of the event portals or buttons to enter a minigame. The assistant should explain that this is the selection room and help the user choose between the 2026 Olympics, 2026 World Cup, and 2026 Super Bowl experiences.";

            case "olympic":
                return "This is the 2026 Olympics scene. It is a bobsleigh-style minigame where the user tilts their head to steer and stay on track. The assistant should explain that the goal is to follow the course, keep control, and use head movement to guide the ride.";

            case "soccer":
                return "This is the 2026 World Cup scene. The user can either act like a goalie and dodge or block incoming soccer balls, or interact with the ball and kick it toward the goal depending on the situation in the scene. The assistant should explain the current objective clearly and encourage the user to focus on the ball and react quickly.";

            case "superbowl":
                return "This is the 2026 Super Bowl scene. The goal is to throw the football toward the field target or goal area. The assistant should clearly explain that this is a football throwing minigame and tell the user to aim carefully and release the football toward the target.";

            case "EndScene":
                return "This is the ending scene of the Time Capsule experience. It shows that the user has finished exploring the 2026 events and completed the journey. The assistant should sound celebratory, explain that the experience is complete, and reflect on the events and memorabilia the user collected.";

            default:
                return "This is a scene in the Time Capsule AR project. The assistant should help the user understand what they are seeing, what they can do here, and what the next step is.";
        }
    }

    private string ExtractOutputText(string json)
    {
        if (string.IsNullOrEmpty(json)) return "";

        ResponseWrapper wrapper = JsonUtility.FromJson<ResponseWrapper>(json);
        if (wrapper == null || wrapper.output == null) return "";

        StringBuilder sb = new StringBuilder();

        foreach (ResponseItem item in wrapper.output)
        {
            if (item == null || item.content == null) continue;

            foreach (ResponseContent content in item.content)
            {
                if (content != null && !string.IsNullOrEmpty(content.text))
                {
                    sb.Append(content.text);
                }
            }
        }

        return sb.ToString().Trim();
    }

    private IEnumerator PlayTextToSpeech(string textToSpeak)
    {
        if (audioSource == null || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(textToSpeak))
            yield break;

        TextToSpeechRequest ttsRequest = new TextToSpeechRequest
        {
            model = ttsModel,
            voice = ttsVoice,
            input = textToSpeak,
            response_format = "mp3"
        };

        string jsonBody = JsonUtility.ToJson(ttsRequest);

        using (UnityWebRequest request = new UnityWebRequest(TtsApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + request.error + "\n" + request.downloadHandler.text);
                yield break;
            }

            byte[] audioBytes = request.downloadHandler.data;
            if (audioBytes == null || audioBytes.Length == 0)
            {
                Debug.LogError("TTS Error: empty audio response.");
                yield break;
            }

            string tempPath = Path.Combine(Application.persistentDataPath, "tts_reply.mp3");
            File.WriteAllBytes(tempPath, audioBytes);

            using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
            {
                yield return audioRequest.SendWebRequest();

                if (audioRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("AudioClip Load Error: " + audioRequest.error);
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                if (clip == null)
                {
                    Debug.LogError("AudioClip Load Error: clip is null.");
                    yield break;
                }

                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    [System.Serializable]
    private class OpenAIRequest
    {
        public string model;
        public string input;
    }

    [System.Serializable]
    private class TextToSpeechRequest
    {
        public string model;
        public string voice;
        public string input;
        public string response_format;
    }

    [System.Serializable]
    private class ResponseWrapper
    {
        public ResponseItem[] output;
    }

    [System.Serializable]
    private class ResponseItem
    {
        public ResponseContent[] content;
    }

    [System.Serializable]
    private class ResponseContent
    {
        public string text;
    }
}