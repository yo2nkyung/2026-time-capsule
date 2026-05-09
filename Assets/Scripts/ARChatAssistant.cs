using System;
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
    public Button voiceButton;
    public GameObject chatPanel;

    [Header("OpenAI")]
    public string apiKey = "";
    public string modelName = "gpt-5.4-mini";
    public bool useFakeResponse = true;

    [Header("Voice Output")]
    public bool useTextToSpeech = true;
    public string ttsModel = "gpt-4o-mini-tts";
    public string ttsVoice = "coral";
    public AudioSource audioSource;

    [Header("Voice Input")]
    public bool useVoiceInput = true;
    public string transcriptionModel = "gpt-4o-mini-transcribe";
    public int maxRecordingSeconds = 6;

    [TextArea(4, 8)]
    public string assistantInstructions =
        "You are a friendly conversational guide inside an AR project called Time Capsule. " +
        "Act like an in-world host who helps the user understand the current scene, what the goal is, and what to do next. " +
        "Always give clear minigame instructions when the user is in a playable scene. " +
        "If the user asks how to play, explain the exact goal, controls, score conditions, and end condition for the current minigame. " +
        "Mention the event name when relevant, like the 2026 Olympics, 2026 World Cup, or 2026 Super Bowl. " +
        "If the user is in the AR entry scene, explain the available paths clearly, including collecting memorabilia and entering minigames through portals. " +
        "Respond in plain text only. Keep replies short and natural. " +
        "Most replies should be 1 to 3 short sentences. " +
        "Do not use markdown, bullet points, or asterisks. " +
        "Be scene-aware, specific, encouraging, and easy to follow.";

    private const string ResponsesApiUrl = "https://api.openai.com/v1/responses";
    private const string TtsApiUrl = "https://api.openai.com/v1/audio/speech";
    private const string TranscriptionApiUrl = "https://api.openai.com/v1/audio/transcriptions";

    // mic stuff
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string micDevice = null;

    private void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }

        if (voiceButton != null)
        {
            voiceButton.onClick.AddListener(OnVoiceClicked);
            voiceButton.gameObject.SetActive(false);
            SetVoiceButtonLabel("Talk");
        }

        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }

        if (responseText != null)
        {
            responseText.text = "Ask me what to do in this scene or minigame.";
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // just use the first mic if there is one
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
        }
    }

    public void ToggleChatPanel()
    {
        if (chatPanel == null) return;

        bool newState = !chatPanel.activeSelf;
        chatPanel.SetActive(newState);

        if (voiceButton != null)
        {
            voiceButton.gameObject.SetActive(newState);
            if (!newState)
            {
                SetVoiceButtonLabel("Talk");
            }
        }

        if (newState && responseText != null && string.IsNullOrWhiteSpace(responseText.text))
        {
            responseText.text = "Ask me what to do in this scene or minigame.";
        }
    }

    public void OnSendClicked()
    {
        if (inputField == null) return;

        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        StartCoroutine(HandleUserMessage(userMessage));

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void OnVoiceClicked()
    {
        if (!useVoiceInput)
        {
            if (responseText != null)
                responseText.text = "Voice input is turned off.";
            return;
        }

        if (string.IsNullOrWhiteSpace(micDevice))
        {
            if (responseText != null)
                responseText.text = "No microphone found.";
            return;
        }

        if (!isRecording)
        {
            recordedClip = Microphone.Start(micDevice, false, maxRecordingSeconds, 16000);
            isRecording = true;
            SetVoiceButtonLabel("Stop");

            if (responseText != null)
                responseText.text = "Listening... press Stop to finish.";
        }
        else
        {
            int samplePosition = Microphone.GetPosition(micDevice);
            Microphone.End(micDevice);
            isRecording = false;
            SetVoiceButtonLabel("Talk");

            if (samplePosition <= 0)
            {
                if (responseText != null)
                    responseText.text = "I didn't hear anything. Try again.";
                return;
            }

            StartCoroutine(TranscribeRecordedAudio(samplePosition));
        }
    }

    private IEnumerator HandleUserMessage(string userMessage)
    {
        if (responseText != null)
        {
            responseText.text = "Thinking...";
        }

        if (useFakeResponse)
        {
            HandleFakeResponse(userMessage);
            yield break;
        }

        yield return StartCoroutine(SendToOpenAI(userMessage));
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

            // also read the reply out loud if tts is on
            if (useTextToSpeech && !string.IsNullOrEmpty(parsedText))
            {
                yield return StartCoroutine(PlayTextToSpeech(parsedText));
            }
        }
    }

    private IEnumerator TranscribeRecordedAudio(int samplePosition)
    {
        if (recordedClip == null)
        {
            if (responseText != null)
                responseText.text = "Recording failed.";
            yield break;
        }

        if (responseText != null)
        {
            responseText.text = "Transcribing...";
        }

        byte[] wavData = SavWav.FromAudioClip(recordedClip, samplePosition);
        if (wavData == null || wavData.Length == 0)
        {
            if (responseText != null)
                responseText.text = "Could not process microphone audio.";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("model", transcriptionModel);
        form.AddBinaryData("file", wavData, "voice.wav", "audio/wav");

        using (UnityWebRequest request = UnityWebRequest.Post(TranscriptionApiUrl, form))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (responseText != null)
                {
                    responseText.text = "Transcription error: " + request.error + "\n" + request.downloadHandler.text;
                }
                yield break;
            }

            string transcriptJson = request.downloadHandler.text;
            TranscriptionResponse transcriptResponse = JsonUtility.FromJson<TranscriptionResponse>(transcriptJson);

            if (transcriptResponse == null || string.IsNullOrWhiteSpace(transcriptResponse.text))
            {
                if (responseText != null)
                {
                    responseText.text = "I couldn't understand that. Try again.";
                }
                yield break;
            }

            if (inputField != null)
            {
                inputField.text = "";
            }

            // run the spoken input through the same flow as typed input
            yield return StartCoroutine(HandleUserMessage(transcriptResponse.text));
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
            "Use the scene description to explain the goal, controls, score conditions, end condition, and next step when relevant. " +
            "If the user asks how to play, answer with the actual current scene gameplay flow. " +
            "Keep the answer concise, natural, and scene-aware.";
    }

    private string GetSceneDescription(string sceneName)
    {
        switch (sceneName)
        {
            case "ARScene":
                return "This is the main AR entry scene for Time Capsule. When the app opens, the user gets a camera permission prompt and sees HUD text telling them to point the camera at the 2026 Time Capsule QR code. When ARImageSpawner detects the image, the PortalHub spawns and the HUD text says the Time Capsule is ready to collect. The HUD then tells the user to flick, hold, and aim items into the capsule to collect memories. The assistant image pulses and the assistant chat panel can show a help prompt saying Need help? Tap me anytime. The user can interact with objects in the scene, collect items while TimeCapsuleController tracks progress, and then tap a portal to enter a minigame. Noteblocks act as obstacles the user can hit.";

            case "olympic":
                return "This is the 2026 Olympics bobsled minigame. The user presses the Game Start button, then after a 3 second countdown the track starts moving and the timer starts. The goal is to reach the finish line before time runs out. The user can restart with the Restart button and can leave the minigame at any time with the Leave button.";

            case "soccer":
                return "This is the 2026 World Cup soccer minigame. When the user enters, a menu lets them choose between two modes: Freekick Mode and Goalkeeper Mode. In Freekick Mode, the user stands at the middle of the court, can walk around the field, and when they contact the ball it is automatically kicked and flies away with sound. In Goalkeeper Mode, the user stands in front of the goal post and gets 5 chances to block balls that are thrown toward random points. Each successful block increases the save count, and when the game resets, the score also resets.";
            case "superbowl":
                return "This is the 2026 Super Bowl football minigame. When the user presses the Start button, a 30 second timer begins. The user must pick up 5 footballs from the ground and throw them. If a ball reaches the touchdown zone or goes into the goalpost, the user scores points. The game ends when the user has thrown all 5 balls or when the 30 second timer runs out, and then the user's score is shown.";

            case "EndScene":
                return "This is the ending scene of the Time Capsule experience. It shows that the user has finished exploring the 2026 events and completed the journey. The assistant should sound celebratory, explain that the experience is complete, and reflect on the events and memorabilia the user collected.";

            default:
                return "This is a scene in the Time Capsule AR project. The assistant should help the user understand what they are seeing, what they can do here, and what the next step is.";
        }
    }

    private void SetVoiceButtonLabel(string newLabel)
    {
        if (voiceButton == null) return;

        TextMeshProUGUI tmp = voiceButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = newLabel;
            return;
        }

        Text legacyText = voiceButton.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = newLabel;
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
    private class TranscriptionResponse
    {
        public string text;
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

public static class SavWav
{
    public static byte[] FromAudioClip(AudioClip clip, int sampleCount)
    {
        if (clip == null || sampleCount <= 0) return null;

        int channels = clip.channels;
        int frequency = clip.frequency;

        float[] samples = new float[sampleCount * channels];
        clip.GetData(samples, 0);

        byte[] wav = ConvertAudioClipDataToInt16ByteArray(samples, sampleCount * channels);

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + wav.Length);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2);
            writer.Write((ushort)(channels * 2));
            writer.Write((ushort)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(wav.Length);
            writer.Write(wav);

            writer.Flush();
            return stream.ToArray();
        }
    }

    private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data, int length)
    {
        byte[] bytes = new byte[length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < length; i++)
        {
            short value = (short)Mathf.Clamp(data[i] * rescaleFactor, short.MinValue, short.MaxValue);
            byte[] byteArr = BitConverter.GetBytes(value);
            byteArr.CopyTo(bytes, i * 2);
        }

        return bytes;
    }
}