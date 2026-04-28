# EasySave

> Backup management software — ProSoft Suite v1.1

EasySave is a console application built with .NET 10 that lets you define, manage, and execute backup jobs with full/differential strategies, real-time state tracking, and configurable JSON or XML logging.

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
- Sequential or selective execution from the interactive menu or via CLI arguments
- Configurable log format: **JSON** (default) or **XML** — switchable at runtime from the Settings menu
- Real-time state file (`state.json` or `state.xml`) updated after every file transfer
- Daily log file (`YYYY-MM-DD.json` or `.xml`) written by the `EasyLog` shared library
- Bilingual UI: **French** and **English**

---

## Project structure

```
CESI-Genie-Logiciel.slnx
├── EasySave.Console/       # Entry point — CLI parsing, interactive menu
├── EasySave.Core/          # Business logic — backup strategies, state, config
├── EasyLog/                # Shared DLL — configurable JSON/XML logging
└── EasySave.Tests/         # xUnit test suite
```

### Key classes

| Class | Project | Role |
|---|---|---|
| `Program` | Console | Entry point, CLI argument parsing, serializer wiring |
| `ConsoleMenu` | Console | Interactive menu (includes Settings sub-menu) |
| `BackupManager` | Core | Orchestrates job execution (Singleton) |
| `ConfigManager` | Core | Loads/saves `AppConfig` with migration from v1.0 format (Singleton) |
| `AppConfig` | Core | Holds jobs + log format preference |
| `StateManager` | Core | Updates and persists state file to `%AppData%\EasySave\` (Singleton) |
| `FullBackup` | Core | Full backup strategy |
| `DifferentialBackup` | Core | Differential backup strategy |
| `IBusinessSoftwareDetector` | Core | Strategy interface for detecting a running business software (v2.0) |
| `ProcessDetector` | Core | Concrete detector using `Process.GetProcessesByName()` |
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

### Interactive mode

```bash
dotnet run --project EasySave.Console
```

Navigate the menu to add jobs, run them, or change the log format in **Settings (6)**.

### CLI mode

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

## UML diagrams

UML diagrams (class, sequence, activity, use case) are available in the `/Diagrams/` directory.

---

## Versioning

| Version | Description |
|---|---|
| 1.0 | Console application, full/differential backup, EasyLog DLL, JSON logs |
| **1.1** | **Unlimited jobs, JSON/XML log format choice, `%AppData%` state path, `EncryptionTime` field, v2.0 groundwork** |
| 2.0 | WPF GUI (MVVM), encryption support, business software detection |
| 3.0 | Parallel execution, pause/stop/play, priority files, remote log centralisation |

See [`RELEASE_NOTES_v1.1.md`](./RELEASE_NOTES_v1.1.md) for the full v1.1 changelog.

---

## License

© ProSoft — All rights reserved.
Unit price: €200 excl. VAT — Annual maintenance contract available (12% of purchase price).
