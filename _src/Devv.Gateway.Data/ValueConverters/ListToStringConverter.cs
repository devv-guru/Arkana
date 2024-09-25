using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Devv.Gateway.Data.ValueConverters;

public class ListToStringConverter : ValueConverter<List<string>, string>
{
    public ListToStringConverter(ConverterMappingHints? mappingHints = null)
        : base(
            list => JsonSerializer.Serialize(list, (JsonSerializerOptions)null),
            json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions)null))
    {
    }
}