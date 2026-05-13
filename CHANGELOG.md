# Changelog

All notable changes to EasySave are documented in this file.
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [3.0.0] — 2026-05-13

### Added
- **Parallel execution** — all backup jobs now run concurrently using `Task.Run`
- **Play / Pause / Stop** controls per job via `CancellationToken` + `ManualResetEventSlim`; pause takes effect after the current file finishes
- **Priority file extensions** — files matching configured extensions are always transferred before non-priority files across all parallel jobs (shared `SemaphoreSlim` gate)
- **Large file throttle** — at most one file above the configured size threshold (n KB) can be transferred simultaneously across all jobs
- **AES-256 encryption** — new `EasySave.CryptoSoft` project with AES-256 CBC + PBKDF2 key derivation; replaces the previous XOR implementation
- **CryptoSoft mono-instance** — `cryptosoft.exe` uses a named system `Mutex` to prevent concurrent execution; `CryptoSoftRunner` additionally serialises calls within a single EasySave process via `SemaphoreSlim`
- **Business software auto-pause** — when the configured business software is detected, all active jobs pause automatically and resume when it stops; events are logged
- **Docker log server** — new Docker service (`log-server`) for centralised log collection; `LogForwarder` POSTs entries via HTTP
- **Log destination setting** — `local` / `remote` / `both` switchable at runtime from Settings
- **Log server URL setting** — configurable in GUI Settings and console Settings menu
- **Large file threshold setting** — configurable in KB (0 = disabled)
- **Priority extensions list** — add/remove from GUI Settings or console Settings menu
- **Encrypted extensions list** — controls which file types are encrypted by CryptoSoft
- **FR/EN language switching in GUI** — `LocalizationService` singleton; toggle button in the header bar refreshes all bindings instantly
- **`NavigationService`** — singleton driving the `ContentControl` view area in the main window
- **`StatusService`** — singleton broadcasting status bar messages; messages are localised
- New `AppConfig` fields: `LogDestination`, `LogServerUrl`, `LargeFileSizeKb`, `PriorityExtensions`, `EncryptedExtensions`
- `BackupState.PlaybackState` property for real-time playback state tracking
- `IBackupStrategy.SetCryptoService()` — runtime injection of the crypto service into strategies

### Changed
- `BackupManager.RunAll()` now runs jobs **in parallel** instead of sequentially
- `BackupStrategyBase.CopyFile()` respects `CancellationToken` and `ManualResetEventSlim` pause gate
- `SettingsViewModel` fully rewritten — manages all v3.0 config fields and rewires Logger / LogForwarder / ProcessDetector on save
- `SettingsView.xaml` wrapped in `ScrollViewer`, all labels and buttons bound to `LocalizationService`
- `BackupJobsView.xaml` — all buttons bound to `LocalizationService`
- `StatusService` messages use `LocalizationService` strings

### Fixed
- `IBackupStrategy` interface was missing `SetCryptoService()` — added to fix build error
- `AppConfig` was missing `EncryptedExtensions` field — added for Settings persistence
- Logger, LogForwarder, and ProcessDetector were never wired at startup — fixed in `App.xaml.cs` and `Program.cs`
- CI `dotnet format` whitespace violations in `LocalizationService.cs` and `StatusService.cs`

---

## [2.0.0]

### Added
- WPF GUI application (`EasySave.GUI`) with MVVM architecture
- Unlimited backup jobs (removed 5-job limit from v1.0)
- CryptoSoft integration — file encryption after transfer for configured extensions
- `EncryptionTime` field in log entries (ms; 0 = no encryption, < 0 = error)
- Business software detection — blocks job launch when configured process is running
- `IBusinessSoftwareDetector` interface + `ProcessDetector` implementation
- `ICryptoService` interface + `CryptoSoftRunner` implementation

---

## [1.1.0]

### Added
- Configurable log format: **JSON** or **XML** — switchable at runtime from Settings menu
- `ILogSerializer` strategy interface with `JsonLogSerializer` and `XmlLogSerializer`
- `EncryptionTime` field added to `LogEntry`
- State and log files moved to `%AppData%\EasySave\` (no more hardcoded `C:\temp\`)
- Unlimited backup jobs

### Changed
- `config.json` format updated to full `AppConfig` object (backwards-compatible with v1.0 plain array)

---

## [1.0.0]

### Added
- Console application built on .NET
- Up to 5 named backup jobs (name, source, target, type)
- Full and differential backup strategies
- CLI argument parsing: `EasySave.exe 1-3` and `EasySave.exe 1;3`
- Real-time state file (`state.json`) updated after every file transfer
- Daily log file (`YYYY-MM-DD.json`) via the `EasyLog` shared DLL
- Bilingual UI: French and English
