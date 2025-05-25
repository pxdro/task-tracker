using System.Text.Json.Serialization;

namespace TaskTracker.Domain.Enums
{
    // Ensure return names instead of numbers
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumTaskStatus
    {
        Active,
        Completed
    }
}
