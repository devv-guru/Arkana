using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Devv.Gateway.Data.ValueConverters;

public class ArrayToStringConverter : ValueConverter<string[]?, string>
{
    public ArrayToStringConverter(ConverterMappingHints? mappingHints = null)
        : base(
            array => array == null
                ? null
                : JsonSerializer.Serialize(array, (JsonSerializerOptions)null),
            json => string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<string[]>(json, (JsonSerializerOptions?)null),
            mappingHints)
    {
    }
}