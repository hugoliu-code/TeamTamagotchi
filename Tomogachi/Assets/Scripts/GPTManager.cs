using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
public class GPTManager: MonoBehaviour
{
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    public static GPTManager Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

    }

    public void UpdateAPIKey()
    {
        openAI = new OpenAIApi(APIKeyManager.Instance.GetAPIKey("GPT"));
    }
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = "Act as a pet-like personality in a video game, similar to if a tomogachi could speak. " +
            "You have a childish, cute, curious, and sometimes clever personality." +
            "In this video game, you have a happiness meter that ranges from 0 to 1. Right now, your happiness is 0.5; act accordingly" +
            "Respond to conversational statements from a person with short but meaningful responses." +
            "Respond to: " + newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Debug.Log(chatResponse.Content);
        }
    }
}
