using System;
using System.IO;
using System.Linq;
using System.Text;

public class JsonFormatter
{
    private const string INDENT_STRING = "    ";

    public static string FormatJson(string str)
    {
        var indent = 0;
        var quoted = false;
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                    }

                    break;
                case '}':
                case ']':
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                    }

                    sb.Append(ch);
                    break;
                case '"':
                    sb.Append(ch);
                    bool escaped = false;
                    var index = i;
                    while (index > 0 && str[--index] == '\\')
                    {
                        escaped = !escaped;
                    }

                    if (!escaped)
                    {
                        quoted = !quoted;
                    }

                    break;
                case ',':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                    }

                    break;
                case ':':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.Append(" ");
                    }

                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        return sb.ToString();
    }

    public static string CompressJson(string json)
    {
        StringBuilder sb = new StringBuilder();
        using (StringReader reader = new StringReader(json))
        {
            int ch = -1;
            int lastch = -1;
            bool isQuoteStart = false;
            while ((ch = reader.Read()) > -1)
            {
                if ((char) lastch != '\\' && (char) ch == '\"')
                {
                    if (!isQuoteStart)
                    {
                        isQuoteStart = true;
                    }
                    else
                    {
                        isQuoteStart = false;
                    }
                }

                if (!Char.IsWhiteSpace((char) ch) || isQuoteStart)
                {
                    sb.Append((char) ch);
                }

                lastch = ch;
            }
        }

        return sb.ToString();
    }

    public static string FromatOrCompress(string json)
    {
        if (json.Contains("\n"))
        {
            return CompressJson(json);
        }

        return FormatJson(json);
    }
}