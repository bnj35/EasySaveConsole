using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

// Translation service: loads a JSON dictionary and exposes `T(key)`.
// Used by the View (Program) to display localized text.
public static class LanguageService
{
    private static Dictionary<string, string> _translations = new();

    // Charge la langue au démarrage
    public static void Load(string lang = "en")
    {
        // Build the translation file path from the selected language.
        string path = Path.Combine("translate", $"Language{lang.ToUpper()}.json");

        // Fail early if translation file is missing.
        if (!File.Exists(path))
            throw new FileNotFoundException($"Language file not found: {path}");

        // Read the JSON file and deserialize it as a dictionary of key -> translated string.
        string json = File.ReadAllText(path);
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                        ?? new Dictionary<string, string>();
    }

    // Raccourci pratique
    public static string T(string key)
        // Return translated string if present, otherwise show a bracketed fallback.
        => _translations.TryGetValue(key, out var val) ? val : $"[{key}]";
}