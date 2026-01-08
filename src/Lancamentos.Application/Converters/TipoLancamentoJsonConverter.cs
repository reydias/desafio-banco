using System.Text.Json;
using System.Text.Json.Serialization;
using Lancamentos.Domain.Entities;

namespace Lancamentos.Application.Converters;

public class TipoLancamentoJsonConverter : JsonConverter<TipoLancamento>
{
    public override TipoLancamento Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value?.ToUpper() switch
        {
            "C" => TipoLancamento.Credito,
            "D" => TipoLancamento.Debito,
            _ => throw new JsonException($"Valor inválido para TipoLancamento: {value}. Use 'C' para Crédito ou 'D' para Débito.")
        };
    }

    public override void Write(Utf8JsonWriter writer, TipoLancamento value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            TipoLancamento.Credito => "C",
            TipoLancamento.Debito => "D",
            _ => throw new JsonException($"Valor inválido do enum TipoLancamento: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}


