using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class LLM : MonoBehaviour
{
    [Header("LLM Settings")]
    [SerializeField] private string apiUrl = "https://macbookair.tail45c10e.ts.net/v1/chat/completions";
    [SerializeField] private string apiKey = ""; // Leave empty for LMStudio local deployment
    [SerializeField] private string systemPrompt = "Chỉ trả lời với tôi bằng tiếng việt. Xưng hô Ta, ngươi, Bạn là Huyền Vũ (Thần Quy) của thế giới này. Bạn cực kỳ tức giận, cục xúc.";
    [SerializeField] private string modelName = "gemma-3-4b-it"; // Model name in LM Studio
    
    [Header("UI Settings")]
    [SerializeField] private GameObject inputModalPanel;
    [SerializeField] private InputField textInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Text responseText;
    [SerializeField] private KeyCode activationKey = KeyCode.T;
    
    private bool isProcessing = false;
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // Add these fields to control cursor and game state
    private bool wasGamePaused = false;
    private bool wasCursorVisible = false;
    private CursorLockMode previousLockMode;
    
    // Add this field to the class
    private Coroutine hideResponseCoroutine = null;
    
    [System.Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
        
        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
    
    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature = 0.7f;
        public int max_tokens = 150;
        
        public ChatRequest(string modelName, List<ChatMessage> messages)
        {
            this.model = modelName;
            this.messages = messages;
        }
    }
    
    [System.Serializable]
    private class ChatResponse
    {
        public Choice[] choices;
    }
    
    [System.Serializable]
    private class Choice
    {
        public ChatMessage message;
    }
    
    void Start()
    {
        // Initialize conversation history with system prompt
        conversationHistory.Add(new ChatMessage("system", systemPrompt));
        
        // Set up UI elements
        if (inputModalPanel != null)
            inputModalPanel.SetActive(false);
        
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
        
        // Clear response text initially
        if (responseText != null)
            responseText.text = "";
        
        Debug.Log("LLM NPC system initialized. Press " + activationKey + " to open chat window.");
    }
    
    void Update()
    {
        // Open input modal when activation key is pressed
        if (Input.GetKeyDown(activationKey) && !isProcessing && !inputModalPanel.activeSelf)
        {
            OpenInputModal();
        }
        
        // Close modal with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && inputModalPanel.activeSelf)
        {
            CloseModal();
        }
    }
    
    void OpenInputModal()
    {
        // Store current game state
        wasGamePaused = Time.timeScale == 0;
        wasCursorVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;
        
        // Pause the game
        Time.timeScale = 0;
        
        // Show and unlock the cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Show the input modal
        inputModalPanel.SetActive(true);
        
        // Clear previous input and focus
        textInputField.text = "";
        textInputField.ActivateInputField();
        textInputField.Select();
    }
    
    public void OnSubmitClicked()
    {
        string userInput = textInputField.text.Trim();
        
        if (!string.IsNullOrEmpty(userInput))
        {
            // Hide the input modal
            inputModalPanel.SetActive(false);
            
            // Process the input
            StartCoroutine(ProcessUserInput(userInput));
            
            // Restore game state and cursor
            RestoreGameState();
        }
    }
    
    // Add a method to close the modal without submitting (optional)
    public void CloseModal()
    {
        inputModalPanel.SetActive(false);
        RestoreGameState();
    }
    
    // Add a helper method to restore game state
    private void RestoreGameState()
    {
        // Restore time scale if it wasn't paused before
        if (!wasGamePaused)
            Time.timeScale = 1;
        
        // Restore cursor state
        Cursor.visible = wasCursorVisible;
        Cursor.lockState = previousLockMode;
    }
    
    IEnumerator ProcessUserInput(string userInput)
    {
        isProcessing = true;
        
        Debug.Log("User input: \"" + userInput + "\"");
        
        // Display "thinking..." message while processing
        if (responseText != null)
            responseText.text = "Suy nghĩ...";
        
        // Send to LLM
        yield return SendToLLM(userInput);
        
        isProcessing = false;
    }
    
    IEnumerator SendToLLM(string userInput)
    {
        // Add user message to conversation history
        conversationHistory.Add(new ChatMessage("user", userInput));
        
        // Create the request object with the model name
        ChatRequest requestData = new ChatRequest(modelName, conversationHistory);
        string jsonRequestData = JsonUtility.ToJson(requestData);
        
        Debug.Log("Sending to LLM: " + jsonRequestData);
        
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(apiKey))
                request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            
            Debug.Log("Sending request to LM Studio...");
            yield return request.SendWebRequest();
            
            string llmResponse = null;
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("LLM Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                llmResponse = "Sorry, I couldn't process your request.";
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Raw LLM response: " + jsonResponse);
                
                try {
                    ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                    
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        llmResponse = response.choices[0].message.content;
                        Debug.Log("LLM Response: " + llmResponse);
                        
                        // Add assistant response to conversation history
                        conversationHistory.Add(new ChatMessage("assistant", llmResponse));
                    }
                    else {
                        Debug.LogError("No choices in response");
                        llmResponse = "I don't know how to respond to that.";
                    }
                }
                catch (Exception e) {
                    Debug.LogError("Error parsing LLM response: " + e.Message);
                    llmResponse = "Error processing response.";
                }
            }
            
            // Display the LLM response in the UI
            if (responseText != null && !string.IsNullOrEmpty(llmResponse))
            {
                responseText.text = llmResponse;
                
                // Cancel any existing hide coroutine
                if (hideResponseCoroutine != null)
                {
                    StopCoroutine(hideResponseCoroutine);
                }
                
                // Start a new hide coroutine
                hideResponseCoroutine = StartCoroutine(HideResponseAfterDelay(5.0f));
            }
        }
    }
    
    // Add this coroutine to hide the response text after a delay
    IEnumerator HideResponseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Fade out the text
        if (responseText != null)
        {
            // Get the original color
            Color originalColor = responseText.color;
            
            // Fade out over 1 second
            float fadeTime = 1.0f;
            float elapsedTime = 0;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                responseText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // Hide text completely and reset color for next time
            responseText.text = "";
            responseText.color = originalColor;
        }
        
        hideResponseCoroutine = null;
    }

    void OnDestroy()
    {
        // Clean up any running coroutines
        if (hideResponseCoroutine != null)
        {
            StopCoroutine(hideResponseCoroutine);
        }
    }
}