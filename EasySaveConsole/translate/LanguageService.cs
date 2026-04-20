using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public static class LanguageService
{
    private static Dictionary<string, string> _translations = new();

    // Charge la langue au démarrage
    public static void Load(string lang = "en")
    {
        string path = Path.Combine("translate", $"Language{lang.ToUpper()}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Language file not found: {path}");

        string json = File.ReadAllText(path);
        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                        ?? new Dictionary<string, string>();
    }

    // Raccourci pratique
    public static string T(string key)
        => _translations.TryGetValue(key, out var val) ? val : $"[{key}]";
}