using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AiFlashcardGenerator.Models
{
    public class Flashcard
    {
        // Use JsonPropertyName to ensure correct deserialization from the AI-generated JSON
        [JsonPropertyName("front")]
        public string Front { get; set; } = string.Empty;

        [JsonPropertyName("back")]
        public string Back { get; set; } = string.Empty;
    }

    public class FlashcardList
    {
        // This helper class is used because the Gemini API returns a JSON array of flashcards,
        // but the structured output requires a top-level object wrapper in the schema,
        // which we can map to a property named "cards".
        [JsonPropertyName("cards")]
        public Flashcard[] Cards { get; set; } = [];
    }
}
