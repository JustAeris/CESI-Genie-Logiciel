namespace EasySave.Console;

// Parse les arguments de ligne de commande pour sélectionner des jobs par index.
public static class ArgParser
{
    // Convertit un argument de sélection en tableau d'index 1-based.
    // "1-3" → [1, 2, 3]   |   "1;3" → [1, 3]   |   "2" → [2]
    public static int[] Parse(string arg)
    {
        // Format plage : "1-3" → développe en [1, 2, 3]
        if (arg.Contains('-'))
        {
            var parts = arg.Split('-', 2);
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());
            return Enumerable.Range(start, end - start + 1).ToArray();
        }

        // Format liste : "1;3" → [1, 3] ; ou index simple : "2" → [2]
        return arg.Split(';')
                  .Select(s => int.Parse(s.Trim()))
                  .ToArray();
    }
}
