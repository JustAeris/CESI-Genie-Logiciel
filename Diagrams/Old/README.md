# Diagrammes UML — EasySave v1.0

| Fichier | Description |
|---|---|
| `architecture.png` | Vue inter-projets : Console, Core, EasyLog |
| `classes.png` | Diagramme de classes détaillé par projet |
| `sequence_run.png` | Séquence d'exécution d'un travail de sauvegarde |
| `sequence_startup.png` | Séquence de démarrage et chargement de la configuration |
| `activity.png` | Diagramme d'activité — flux complet de l'application |

## Patterns utilisés

- **Singleton** — `Logger`, `BackupManager`, `StateManager`, `ConfigManager`
- **Strategy** — `IBackupStrategy` / `FullBackup` / `DifferentialBackup`
