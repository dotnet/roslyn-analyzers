using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "The year is 2024 and the month is October.";
        string pattern = @"\d+";

        // Example 1: The problematic pattern that should trigger the analyzer
        if (Regex.IsMatch(input, pattern))
        {
            Match m = Regex.Match(input, pattern);
            Console.WriteLine($"Found match: {m.Value}");
        }

        // Example 2: Already using the recommended pattern (no diagnostic)
        if (Regex.Match(input, pattern) is { Success: true } m2)
        {
            Console.WriteLine($"Found match: {m2.Value}");
        }

        // Example 3: IsMatch without corresponding Match (no diagnostic)
        if (Regex.IsMatch(input, @"[A-Z]"))
        {
            Console.WriteLine("Contains uppercase letters");
        }
    }
}
