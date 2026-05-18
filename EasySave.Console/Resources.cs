namespace EasySave.Console;

// Langues d'interface supportées par la console
public enum Language { EN, FR }

// Fournit les chaînes d'interface localisées pour EN et FR.
// Utiliser Get() pour récupérer une chaîne par clé dans la langue active.
public static class Resources
{
    // Langue active au démarrage — modifiable à chaud via le menu console
    public static Language Current { get; set; } = Language.EN;

    // Dictionnaire de toutes les chaînes localisées, indexées par clé puis par langue
    private static readonly Dictionary<string, Dictionary<Language, string>> _strings = new()
    {
        ["menu.title"] = new() { [Language.EN] = "=== EasySave ===", [Language.FR] = "=== EasySave ===" },
        ["menu.list"] = new() { [Language.EN] = "1. List backup jobs", [Language.FR] = "1. Lister les travaux" },
        ["menu.add"] = new() { [Language.EN] = "2. Add backup job", [Language.FR] = "2. Ajouter un travail" },
        ["menu.run"] = new() { [Language.EN] = "3. Run a job", [Language.FR] = "3. Lancer un travail" },
        ["menu.runall"] = new() { [Language.EN] = "4. Run all jobs", [Language.FR] = "4. Lancer tous les travaux" },
        ["menu.language"] = new() { [Language.EN] = "5. Change language", [Language.FR] = "5. Changer de langue" },
        ["menu.exit"] = new() { [Language.EN] = "7. Exit", [Language.FR] = "7. Quitter" },
        ["menu.choice"] = new() { [Language.EN] = "Your choice: ", [Language.FR] = "Votre choix : " },
        ["job.name"] = new() { [Language.EN] = "Job name: ", [Language.FR] = "Nom du travail : " },
        ["job.source"] = new() { [Language.EN] = "Source directory: ", [Language.FR] = "Répertoire source : " },
        ["job.target"] = new() { [Language.EN] = "Target directory: ", [Language.FR] = "Répertoire cible : " },
        ["job.type"] = new() { [Language.EN] = "Type (1=Full, 2=Diff): ", [Language.FR] = "Type (1=Complet, 2=Diff) : " },
        ["job.index"] = new() { [Language.EN] = "Job number: ", [Language.FR] = "Numéro du travail : " },
        ["job.added"] = new() { [Language.EN] = "Job added.", [Language.FR] = "Travail ajouté." },
        ["job.done"] = new() { [Language.EN] = "Done.", [Language.FR] = "Terminé." },
        ["job.none"] = new() { [Language.EN] = "No jobs configured.", [Language.FR] = "Aucun travail configuré." },
        ["error.invalid"] = new() { [Language.EN] = "Invalid input.", [Language.FR] = "Entrée invalide." },
        ["lang.choice"] = new() { [Language.EN] = "Language (EN/FR): ", [Language.FR] = "Langue (EN/FR) : " },
        ["menu.settings"] = new() { [Language.EN] = "6. Settings", [Language.FR] = "6. Paramètres" },
        ["settings.title"] = new() { [Language.EN] = "=== Settings ===", [Language.FR] = "=== Paramètres ===" },
        ["settings.format"] = new() { [Language.EN] = "Log format", [Language.FR] = "Format du log" },
        ["settings.format.choose"] = new() { [Language.EN] = "Choose format (json/xml): ", [Language.FR] = "Choisir le format (json/xml) : " },
        ["settings.format.updated"] = new() { [Language.EN] = "Log format updated.", [Language.FR] = "Format du log mis à jour." },
        ["settings.format.invalid"] = new() { [Language.EN] = "Invalid format. Use 'json' or 'xml'.", [Language.FR] = "Format invalide. Utilisez 'json' ou 'xml'." },
    };

    // Retourne la chaîne localisée pour la clé donnée dans la langue active.
    // Retourne la clé elle-même si elle n'est pas trouvée — évite les plantages silencieux.
    public static string Get(string key) =>
        _strings.TryGetValue(key, out var map) && map.TryGetValue(Current, out var val)
            ? val
            : key;
}
