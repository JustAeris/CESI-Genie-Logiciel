# EasySave v3.0 — Manuel Utilisateur

## Démarrage

### Mode GUI
```bash
dotnet run --project EasySave.GUI
```

### Mode Console
```bash
dotnet run --project EasySave.Console
```

---

## Gestion des sauvegardes

### Ajouter une tâche
1. Cliquer sur **"Ajouter"**
2. Renseigner le nom, le dossier source et le dossier cible
3. Choisir le type : **Full** ou **Differential**
4. Cliquer sur **"Sauvegarder"**

### Lancer une sauvegarde
- **Une tâche** → sélectionner et cliquer sur **"Lancer"**
- **Toutes les tâches** → cliquer sur **"Lancer tout"**

### Contrôler une sauvegarde
| Action | Description |
|--------|-------------|
| **Pause** | Suspend après le fichier en cours |
| **Resume** | Reprend la sauvegarde |
| **Stop** | Arrête immédiatement |

---

## Paramètres

| Paramètre | Description |
|-----------|-------------|
| Format des logs | JSON ou XML |
| Extensions prioritaires | Ex: .pdf, .docx |
| Seuil gros fichiers | En KB |
| Logiciel métier | Nom du processus à détecter |
| Destination logs | local / remote / both |

---

## Logs Docker
1. Démarrer le serveur : `docker-compose up -d`
2. Les logs sont envoyés automatiquement à `http://localhost:5000/logs`

---

## Fichiers de configuration
Stockés dans `%AppData%\EasySave\`
- `config.json` → configuration
- `state.json` → état en temps réel
- `logs/` → journaux quotidiens