namespace EasySave.Console;

/// <summary>
/// Parses command-line job-selection arguments into arrays of job indices.
/// </summary>
public static class ArgParser
{
    /// <summary>
    /// Parses a job-selection argument into 1-based job indices.
    /// "1-3" → [1, 2, 3]   |   "1;3" → [1, 3]   |   "2" → [2]
    /// </summary>
    public static int[] Parse(string arg)
    {
        if (arg.Contains('-'))
        {
            var parts = arg.Split('-', 2);
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());
            return Enumerable.Range(start, end - start + 1).ToArray();
        }

        return arg.Split(';')
                  .Select(s => int.Parse(s.Trim()))
                  .ToArray();
    }
}
