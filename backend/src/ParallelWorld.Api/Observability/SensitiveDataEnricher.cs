using System.Collections;
using Serilog.Core;
using Serilog.Events;

namespace ParallelWorld.Api.Observability;

public sealed class SensitiveDataEnricher : ILogEventEnricher
{
    public const string RedactedValue = "[REDACTED]";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory _)
    {
        foreach (var (propertyName, propertyValue) in logEvent.Properties.ToArray())
        {
            logEvent.AddOrUpdateProperty(
                new LogEventProperty(propertyName, Redact(propertyName, propertyValue)));
        }

        RedactExceptionData(logEvent.Exception);
    }

    private static LogEventPropertyValue Redact(
        string? propertyName,
        LogEventPropertyValue propertyValue)
    {
        if (propertyName is not null && IsSensitiveName(propertyName))
        {
            return new ScalarValue(RedactedValue);
        }

        return propertyValue switch
        {
            SequenceValue sequence => new SequenceValue(
                sequence.Elements.Select(element => Redact(null, element))),
            StructureValue structure => new StructureValue(
                structure.Properties.Select(property =>
                    new LogEventProperty(property.Name, Redact(property.Name, property.Value))),
                structure.TypeTag),
            DictionaryValue dictionary => new DictionaryValue(
                dictionary.Elements.Select(entry => new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                    entry.Key,
                    entry.Key.Value is string key && IsSensitiveName(key)
                        ? new ScalarValue(RedactedValue)
                        : Redact(null, entry.Value)))),
            _ => propertyValue,
        };
    }

    private static void RedactExceptionData(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            RedactDictionary(current.Data, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }
    }

    private static void RedactDictionary(IDictionary dictionary, HashSet<object> visited)
    {
        if (!visited.Add(dictionary))
        {
            return;
        }

        foreach (var key in dictionary.Keys.Cast<object>().ToArray())
        {
            if (key is string name && IsSensitiveName(name))
            {
                dictionary[key] = RedactedValue;
            }
            else if (dictionary[key] is IDictionary nestedDictionary)
            {
                RedactDictionary(nestedDictionary, visited);
            }
        }
    }

    private static bool IsSensitiveName(string propertyName)
    {
        var normalizedName = new string(
            propertyName.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

        return normalizedName.Contains("authorization", StringComparison.Ordinal)
            || normalizedName.Contains("cookie", StringComparison.Ordinal)
            || normalizedName.Contains("connectionstring", StringComparison.Ordinal)
            || normalizedName.Contains("credential", StringComparison.Ordinal)
            || normalizedName.Contains("password", StringComparison.Ordinal)
            || normalizedName.Contains("secret", StringComparison.Ordinal)
            || normalizedName.Contains("apikey", StringComparison.Ordinal)
            || normalizedName == "token"
            || normalizedName.EndsWith("token", StringComparison.Ordinal)
            || normalizedName == "proof"
            || normalizedName.EndsWith("proof", StringComparison.Ordinal)
            || normalizedName is "body" or "payload"
            || normalizedName.EndsWith("body", StringComparison.Ordinal)
            || normalizedName.EndsWith("payload", StringComparison.Ordinal);
    }
}
