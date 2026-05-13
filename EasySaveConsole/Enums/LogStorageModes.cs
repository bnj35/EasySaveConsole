using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogStorageModes
{
    Remote,
    Local,
    Both
}