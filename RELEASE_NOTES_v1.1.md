# EasySave v1.1.0 — Release Notes

## Nouvelles fonctionnalités

### Choix du format de log (JSON ou XML)
- Le format de log est maintenant configurable depuis le menu **Paramètres** (option 6)
- Deux formats disponibles : **JSON** (défaut, rétrocompatible v1.0) et **XML**
- Le choix est persisté dans `config.json` et appliqué immédiatement sans redémarrage
- Logs et fichier state utilisent le même format choisi

### Champ EncryptionTime dans les logs
- Chaque entrée de log `LogEntry` contient désormais un champ `EncryptionTime` (en ms)
- Valeurs : `0` = pas de chiffrement, `> 0` = durée de chiffrement, `< 0` = erreur

### Suppression de la limite de 5 travaux
- Il n'y a plus de limite au nombre de travaux de sauvegarde configurables

## Corrections

### Emplacement du fichier state
- Le fichier `state.json` est désormais créé dans `%AppData%\EasySave\` (et non dans le répertoire de travail courant)

## Rétrocompatibilité

- **Format de log** : JSON par défaut — comportement v1.0 inchangé
- **Migration config** : un fichier `config.json` au format v1.0 (tableau de jobs seul) est automatiquement migré vers le format v1.1 sans erreur ni perte de données

## Emplacements des fichiers

| Fichier | Chemin |
|---|---|
| Configuration | `%AppData%\EasySave\config.json` |
| État | `%AppData%\EasySave\state.json` (ou `.xml`) |
| Logs | `%AppData%\EasySave\logs\yyyy-MM-dd.json` (ou `.xml`) |

## Configuration minimale

- Windows 10 / 11
- .NET 10.0 Runtime

## Groundwork v2.0 inclus

- Interface `IBusinessSoftwareDetector` + implémentation `ProcessDetector` (prêt pour Dev 2 — T14)
- Architecture Strategy (GoF) pour la sérialisation : ajout futur de formats sans modification du code existant
