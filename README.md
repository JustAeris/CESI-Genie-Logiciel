# EasySave

> Backup management software — ProSoft Suite v1.0

EasySave is a console application built with .NET 8 that lets you define, manage, and execute backup jobs with full/differential strategies, real-time state tracking, and daily JSON logging.

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

- Create up to 5 named backup jobs (v1.0)
- Two backup strategies: **full** and **differential**
- Source and target directories on local disks, external drives, or network shares (UNC paths)
- Sequential or selective execution from the interactive menu or via CLI arguments
- Real-time state file (`state.json`) updated after every file transfer
- Daily log file (`YYYY-MM-DD.json`) written by the `EasyLog` shared library
- Bilingual UI: **French** and **English**

---

## Project structure

```
EasySave.sln
├── EasySave.Console/       # Entry point — CLI parsing, interactive menu
├── EasySave.Core/          # Business logic — backup strategies, state, config
└── EasyLog/                # Shared DLL — daily JSON logging (EasyLog.dll)
```

### Key classes

| Class | Project | Role |
|---|---|---|
| `Program` | Console | Entry point, CLI argument parsing |
| `ConsoleMenu` | Console | Interactive menu |
| `BackupManager` | Core | Orchestrates job execution (Singleton) |
| `ConfigManager` | Core | Loads/saves job configuration (Singleton) |
| `StateManager` | Core | Updates and persists `state.json` (Singleton) |
| `FullBackup` | Core | Full backup strategy |
| `DifferentialBackup` | Core | Differential backup strategy |
| `Logger` | EasyLog | Appends entries to the daily log file (Singleton) |

---

## Requirements

- Windows, Linux, or macOS
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Visual Studio 2022 or later (recommended)

---

## Installation

```bash
git clone https://github.com/<your-org>/easysave.git
cd easysave
dotnet build
```

The compiled executable and `EasyLog.dll` will be placed in the respective `bin/` output folders.

---

## Usage

### Interactive mode

```bash
dotnet run --project EasySave.Console
```

The menu lets you add/edit backup jobs and launch one or all of them.

### CLI mode

```bash
# Run jobs 1, 2 and 3 sequentially
EasySave.exe 1-3

# Run jobs 1 and 3 only
EasySave.exe 1;3
```

---

## Configuration files

All files are in **JSON format** with line breaks for readability.

### Default locations

Files are stored relative to the application's working directory to ensure compatibility across client servers. Absolute paths such as `C:\temp\` are avoided by design.

| File | Default path | Description |
|---|---|---|
| `config.json` | `./config/config.json` | Saved backup jobs |
| `state.json` | `./logs/state.json` | Real-time job state |
| `YYYY-MM-DD.json` | `./logs/YYYY-MM-DD.json` | Daily transfer log |

### `state.json` — example

```json
[
  {
    "Name": "Save1",
    "SourceFilePath": "",
    "TargetFilePath": "",
    "State": "END",
    "TotalFilesToCopy": 0,
    "TotalFilesSize": 0,
    "NbFilesLeftToDo": 0,
    "SizeLeft": 0,
    "Progression": 0,
    "Timestamp": "17/12/2020 17:06:52"
  }
]
```

### `YYYY-MM-DD.json` — example

```json
[
  {
    "Name": "Save1",
    "FileSource": "D:\\project\\source\\file.txt",
    "FileTarget": "E:\\backup\\project\\source\\file.txt",
    "FileSize": 174592,
    "FileTransferTime": 38.029,
    "time": "17/12/2020 17:06:49"
  }
]
```

`FileTransferTime` is expressed in milliseconds. A negative value indicates a transfer error.

---

## UML diagrams

UML diagrams (class, sequence, activity, use case) are located in the `/docs/uml/` directory and maintained as PlantUML `.puml` files.

```
docs/
└── uml/
    ├── architecture.puml
    ├── classes.puml
    ├── sequence_run.puml
    ├── sequence_startup.puml
    └── activity.puml
```

---

## Versioning

| Version | Description |
|---|---|
| 1.0 | Console application, up to 5 backup jobs, full/differential, EasyLog DLL |
| 1.1 | Bug fixes (branch `release/1.1`) |
| 2.0 | WPF GUI (MVVM), unlimited jobs, encryption support |
| 3.0 | Parallel execution, pause/stop/play, priority files, Docker log centralisation |

Release notes are available in [`RELEASE_NOTES.md`](./RELEASE_NOTES.md).

---

## License

© ProSoft — All rights reserved.
Unit price: €200 excl. VAT — Annual maintenance contract available (12% of purchase price).
