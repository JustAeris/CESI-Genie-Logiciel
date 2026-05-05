# EasySave v2.0.0 — Release Notes

## Nouvelles fonctionnalités

### Interface graphique WPF (MVVM)
- L'application dispose désormais d'une interface graphique native Windows (WPF) en remplacement du menu console
- Architecture MVVM complète : `ViewModelBase`, `RelayCommand`, `NavigationService`
- Thème CESI : palette de couleurs et typographie aux couleurs de l'établissement
- Deux vues principales accessibles depuis la barre latérale :
  - **Travaux de sauvegarde** : liste des jobs, boutons Ajouter / Supprimer / Exécuter / Tout exécuter
  - **Paramètres** : format de log, nom du logiciel métier, extensions à chiffrer
- Dialogue d'ajout de job (`AddJobDialog`) avec sélecteur de dossier natif
- Notifications toast à la fin de chaque sauvegarde (nom du job + durée en ms)

### Chiffrement via CryptoSoft
- Interface `ICryptoService` (Strategy GoF) — le moteur de chiffrement est interchangeable
- Implémentation `CryptoSoftRunner` : appelle `cryptosoft.exe <chemin>` en sous-processus
- Chaque fichier transféré est chiffré automatiquement si un service de chiffrement est configuré
- Le temps de chiffrement est enregistré dans `EncryptionTime` de chaque entrée de log :
  - `0` = pas de chiffrement
  - `> 0` = durée en millisecondes
  - `< 0` = erreur lors du chiffrement

### Blocage par logiciel métier
- Interface `IBusinessSoftwareDetector` + implémentation `ProcessDetector`
- Si le logiciel métier est détecté en cours d'exécution, le job est immédiatement interrompu
- Un enregistrement de log est émis avec `FileTransferTime = -1` pour tracer le blocage
- Le nom du logiciel métier est configurable depuis la vue **Paramètres** de la GUI

## Extensions à chiffrer
- La vue Paramètres permet de gérer une liste d'extensions de fichiers à soumettre au chiffrement
- Les extensions sont persistées dans `config.json`

## Tests

### Tests d'intégration pipeline backup
- `BackupPipelineTests` : couverture du pipeline complet avec mock `ICryptoService`
- Vérifie le blocage par logiciel métier, le chiffrement, et les entrées de log produites

## Rétrocompatibilité

- **Console** : `EasySave.Console` reste disponible et fonctionnel — la GUI est un projet distinct (`EasySave.GUI`)
- **Configuration** : le format `config.json` v1.1 est repris tel quel ; aucune migration nécessaire
- **Logs** : format JSON et XML inchangés — `EncryptionTime` était déjà présent depuis v1.1

## Emplacements des fichiers

| Fichier | Chemin |
|---|---|
| Configuration | `%AppData%\EasySave\config.json` |
| État | `%AppData%\EasySave\state.json` (ou `.xml`) |
| Logs | `%AppData%\EasySave\logs\yyyy-MM-dd.json` (ou `.xml`) |

## Configuration minimale

- Windows 10 / 11
- .NET 10.0 Runtime
- Pour le chiffrement : `cryptosoft.exe` accessible dans le `PATH` ou dans le répertoire de l'exécutable
