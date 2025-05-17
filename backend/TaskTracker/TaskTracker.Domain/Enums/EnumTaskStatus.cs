using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
