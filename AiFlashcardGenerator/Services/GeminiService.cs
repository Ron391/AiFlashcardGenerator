using AiFlashcardGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiFlashcardGenerator.Services
{
    public class GeminiService
    {
        // IMPORTANT: Replace this with your actual API key for a real application.
        // In this environment, it remains an empty string.
        private const string ApiKey = "";
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key=";
        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Generates flashcards for a given topic using the Gemini API with structured JSON output.
        /// </summary>
        /// <param name="topic">The topic or text to generate flashcards from.</param>
        /// <returns>A list of generated Flashcard objects.</returns>
        public async Task<List<Flashcard>> GenerateFlashcardsAsync(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return new List<Flashcard>();
            }

            // The system prompt guides the model's behavior
            var systemInstruction = new
            {
                parts = new[] {
                    new {
                        text = "You are an expert educational flashcard generator. Based on the user's topic, generate a list of 5 concise flashcards. Each flashcard MUST have a 'front' (term/question) and a 'back' (definition/answer)."
                    }
                }
            };

            // Define the JSON schema for the expected response structure
            var responseSchema = new
            {
                type = "OBJECT",
                properties = new
                {
                    cards = new
                    {
                        type = "ARRAY",
                        description = "A list of flashcards generated for the given topic.",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                front = new { type = "STRING", description = "The question or term for the front of the flashcard." },
                                back = new { type = "STRING", description = "The answer or definition for the back of the flashcard." }
                            },
                            required = new[] { "front", "back" }
                        }
                    }
                },
                required = new[] { "cards" }
            };

            // Construct the full request payload
            var payload = new
            {
                contents = new[] {
                    new {
                        parts = new[] { new { text = $"Generate 5 flashcards about: {topic}" } }
                    }
                },
                systemInstruction = systemInstruction,
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = responseSchema
                }
            };

            // Convert the payload object to a JSON string content
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Make the API call
            var response = await _httpClient.PostAsync(ApiUrl + ApiKey, content);

            response.EnsureSuccessStatusCode();

            // Deserialize the response to get the generated JSON
            var responseJson = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<JsonDocument>(responseJson);

            // Extract the text part which contains the structured JSON string
            var jsonString = apiResponse?
                .RootElement.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            if (jsonString == null)
            {
                throw new InvalidOperationException("Gemini API response did not contain the expected JSON text part.");
            }

            // The JSON string is the structured output we defined with the FlashcardList structure
            var flashcardList = JsonSerializer.Deserialize<FlashcardList>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return flashcardList?.Cards?.ToList() ?? new List<Flashcard>();
        }
    }
}

