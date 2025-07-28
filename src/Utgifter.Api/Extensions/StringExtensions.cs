namespace Utgifter.Api.Extensions;

public static class StringExtensions
{
    public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> str, string start, StringComparison comparison = StringComparison.Ordinal)
    {
        if (str.IsEmpty || start.Length == 0 || start.Length > str.Length)
            return str;

        if (str.StartsWith(start, comparison))
            return str[start.Length..];
        
        
        return str;
    }

    public static string TrimStart(this string str,  StringComparison comparison = StringComparison.Ordinal, params IEnumerable<string> starts)
    {
        var span = str.AsSpan();
        foreach (var start in starts)
        {
            var newSpan = span.TrimStart(start, comparison);
            if (newSpan.Length != span.Length)
            {
                return newSpan.ToString();
            }
        }

        return str;
    }
    
}