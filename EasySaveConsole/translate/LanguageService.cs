using System.Text.Json;

public static class LanguageService
{
    private static Dictionary<string,string> _translation = new () ;

    public static void Load(string lang = "en")
    {
        string path = Path.Combine("translate",$"Language{lang.ToUpper()}.json");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(string.Format(LanguageService.T("error.lang.file.notfound"), path));
        }

        string json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<JsonElement>(json);
        _translation = new Dictionary<string, string>();

        FlattenJson(root, string.Empty, _translation);

    }

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

    public static string T(string key)
    {
        return _translation.TryGetValue(key, out var val) ? val : $"[{key}]";
    }
}