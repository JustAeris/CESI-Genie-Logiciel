# EasySave

> Backup management software — ProSoft Suite v2.0

EasySave is a Windows desktop application (WPF) built with .NET 10 that lets you define, manage, and execute backup jobs with full/differential strategies, CryptoSoft encryption, business software detection, real-time state tracking, and configurable JSON or XML logging.

---

## Table of contents

- [Features](#features)
- [Project structure](#project-structure)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration files](#configuration-files)
- [UML diagrams](#uml-diagrams)
- [Versioning](#versioning)
- [License](#license)

---

## Features

- Unlimited named backup jobs
- Two backup strategies: **full** and **differential**
- Source and target directories on local disks, external drives, or network shares (UNC paths)
- **WPF graphical interface** (MVVM, CESI theme) — add, remove, run jobs, toast notifications
- **CryptoSoft encryption** — files are encrypted after transfer via `cryptosoft.exe`; encryption time logged
- **Business software detection** — backup is blocked while the configured business software is running
- Configurable log format: **JSON** (default) or **XML** — switchable at runtime from the Settings view
- Real-time state file (`state.json` or `state.xml`) updated after every file transfer
- Daily log file (`YYYY-MM-DD.json` or `.xml`) written by the `EasyLog` shared library
- Console entry point (`EasySave.Console`) still available for CLI usage

---

## Project structure

```
CESI-Genie-Logiciel.slnx
├── EasySave.GUI/           # WPF desktop application (MVVM, CESI theme) — main entry point
├── EasySave.Console/       # CLI entry point — argument parsing, interactive menu
├── EasySave.Core/          # Business logic — backup strategies, state, config, crypto
├── EasyLog/                # Shared DLL — configurable JSON/XML logging
└── EasySave.Tests/         # xUnit test suite
```

### Key classes

| Class | Project | Role |
|---|---|---|
| `App` / `MainWindow` | GUI | WPF application shell, navigation host |
| `BackupJobsViewModel` | GUI | Lists jobs, runs selected/all, add/remove with dialog |
| `SettingsViewModel` | GUI | Log format, business software name, encrypted extensions |
| `ToastNotification` | GUI | Toast popup on job completion |
| `Program` | Console | Entry point, CLI argument parsing, serializer wiring |
| `ConsoleMenu` | Console | Interactive menu (includes Settings sub-menu) |
| `BackupManager` | Core | Orchestrates job execution (Singleton) |
| `BackupStrategyBase` | Core | Copies files, calls `ICryptoService`, writes log + state |
| `ICryptoService` | Core | Strategy interface for file encryption |
| `CryptoSoftRunner` | Core | Calls `cryptosoft.exe`; returns elapsed ms or -1 on error |
| `IBusinessSoftwareDetector` | Core | Strategy interface — returns `true` if target process is running |
| `ProcessDetector` | Core | Concrete detector using `Process.GetProcessesByName()` |
| `ConfigManager` | Core | Loads/saves `AppConfig` with migration from v1.0 format (Singleton) |
| `AppConfig` | Core | Holds jobs + log format preference |
| `StateManager` | Core | Updates and persists state file to `%AppData%\EasySave\` (Singleton) |
| `FullBackup` | Core | Full backup strategy |
| `DifferentialBackup` | Core | Differential backup strategy |
| `Logger` | EasyLog | Appends entries to the daily log file (Singleton) |
| `ILogSerializer` | EasyLog | Strategy interface for log serialization |
| `JsonLogSerializer` | EasyLog | JSON implementation (default, v1.0 compatible) |
| `XmlLogSerializer` | EasyLog | XML implementation |

---

## Requirements

- Windows 10 / 11
- [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 / JetBrains Rider (recommended)

---

## Installation

```bash
git clone https://github.com/JustAeris/CESI-Genie-Logiciel.git
cd CESI-Genie-Logiciel
dotnet build
```

---

## Usage

### GUI mode (recommended)

```bash
dotnet run --project EasySave.GUI
```

Use the sidebar to navigate between **Backup Jobs** and **Settings**.
Jobs can be added, removed, run individually or all at once.
A toast notification confirms completion with the elapsed time.

### Console — interactive mode

```bash
dotnet run --project EasySave.Console
```

Navigate the menu to add jobs, run them, or change the log format in **Settings (6)**.

### Console — CLI mode

```bash
# Run jobs 1, 2 and 3 sequentially
EasySave.Console.exe 1-3

# Run jobs 1 and 3 only
EasySave.Console.exe 1;3
```

---

## Configuration files

All configuration and log files are stored in `%AppData%\EasySave\`.

| File | Path | Description |
|---|---|---|
| `config.json` | `%AppData%\EasySave\config.json` | Backup jobs + log format preference |
| `state.json` / `state.xml` | `%AppData%\EasySave\state.*` | Real-time job state |
| `YYYY-MM-DD.json` / `.xml` | `%AppData%\EasySave\logs\` | Daily transfer log |

### `config.json` — v1.1 format

```json
{
  "Jobs": [
    {
      "Name": "Save1",
      "SourceDir": "D:\\project\\source",
      "TargetDir": "E:\\backup\\project",
      "Type": "Full"
    }
  ],
  "LogFormat": "json"
}
```

> **Retrocompat:** a v1.0 config (plain array of jobs) is automatically migrated on first load.

### `YYYY-MM-DD.json` — log entry example

```json
[
  {
    "Name": "Save1",
    "FileSource": "D:\\project\\source\\file.txt",
    "FileTarget": "E:\\backup\\project\\file.txt",
    "FileSize": 174592,
    "FileTransferTime": 38,
    "time": "2026-04-28 17:06:49",
    "EncryptionTime": 0
  }
]
```

`FileTransferTime` and `EncryptionTime` are in milliseconds. `EncryptionTime` = 0 (no encryption), > 0 (duration), < 0 (error).

---
## Déploiement Docker (logs centralisés)

EasySave v3.0 supporte la centralisation des logs vers un serveur Docker distant.

### Démarrer le serveur de logs

```bash
docker-compose up -d
```

### Configuration

Dans `config.json`, configurez la destination des logs :

```json
{
  "LogDestination": "both",
  "LogServerUrl": "http://localhost:5000/logs"
}
```

| Valeur | Description |
|--------|-------------|
| `local` | Logs locaux uniquement (défaut) |
| `remote` | Logs distants uniquement |
| `both` | Logs locaux ET distants |

### Mode dégradé
Si le serveur Docker est inaccessible, les logs locaux sont **toujours garantis**.
---

## UML diagrams

UML diagrams (class, sequence, activity, use case) are available in the `/Diagrams/` directory.

---

## Versioning

| Version | Description |
|---|---|
| 1.0 | Console application, full/differential backup, EasyLog DLL, JSON logs |
| 1.1 | Unlimited jobs, JSON/XML log format choice, `%AppData%` state path, `EncryptionTime` field |
| **2.0** | **WPF GUI (MVVM, CESI theme), CryptoSoft encryption, business software detection** |
| 3.0 | Parallel execution, pause/stop/play, priority files, remote log centralisation |

See [`RELEASE_NOTES_v2.0.md`](./RELEASE_NOTES_v2.0.md) for the full v2.0 changelog.
See [`RELEASE_NOTES_v1.1.md`](./RELEASE_NOTES_v1.1.md) for the v1.1 changelog.

---

## License

© ProSoft — All rights reserved.
Unit price: €200 excl. VAT — Annual maintenance contract available (12% of purchase price).
