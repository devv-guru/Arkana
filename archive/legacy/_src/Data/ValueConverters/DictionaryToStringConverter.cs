using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Data.ValueConverters;

public class DictionaryToStringConverter : ValueConverter<Dictionary<string, string>, string>
{
    public DictionaryToStringConverter()
        : base(
            dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions)null),
            json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions)null))
    {
    }
}