using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// Translation service: loads a JSON dict and exposes `T(key)`.
public static class LanguageService
{
    private static Dictionary<string, string> _translations = new();

    // Loads the specified language file (default: "en")
    public static void Load(string lang = "en")
    {
        string path = Path.Combine("translate", $"Language{lang.ToUpper()}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Language file not found: {path}");

        string json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<JsonElement>(json);
        _translations = new Dictionary<string, string>();

        // Flattens JSON (e.g., "Menu": {"File": "X"} -> "Menu.File": "X")
        FlattenJson(root, string.Empty, _translations);
    }

    // Recursively flattens nested JSON objects into dot-separated keys
    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> dict)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenJson(property.Value, key, dict); 
                }
                break;
                
            case JsonValueKind.String:
                dict[prefix] = element.GetString() ?? string.Empty;
                break;

            default:
                dict[prefix] = element.ToString();
                break;
        }
    }
    
    // Returns the translation, or falls back to "[key]" if missing
    public static string T(string key)
        => _translations.TryGetValue(key, out var val) ? val : $"[{key}]";
}