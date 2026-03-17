namespace StrivoIngestPublish;

/// <summary>
/// Provides CSV parsing and JSON message building utilities.
/// </summary>
internal static class CsvMessageBuilder
{
    /// <summary>
    /// Builds a JSON message payload from a CSV header line and a data line.
    /// </summary>
    internal static string BuildMessage(string headerLine, string dataLine, string sourceBlobName)
    {
        var headers = ParseCsvLine(headerLine);
        var values = ParseCsvLine(dataLine);

        var fields = new List<string>(headers.Length + 1)
        {
            $"\"source\":\"{EscapeJson(sourceBlobName)}\""
        };

        for (int i = 0; i < headers.Length; i++)
        {
            string value = i < values.Length ? values[i] : string.Empty;
            fields.Add($"\"{EscapeJson(headers[i])}\":\"{EscapeJson(value)}\"");
        }

        return "{" + string.Join(",", fields) + "}";
    }

    /// <summary>
    /// Parses a single CSV line, respecting RFC 4180 quoting rules.
    /// </summary>
    internal static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return [.. fields];
    }

    internal static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
