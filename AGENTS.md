# Repository Guidelines

## Project Structure & Module Organization
- Source lives under `Assets/_Project`: `Core` holds shared services (ServiceLocator, GameEvent, Command pipeline), `Features` layers gameplay systems, and `Features/Templates` store canonical sample implementations to reference not fork.
- Scene and sandbox assets sit in `Assets/_Project/Scenes`, `_Sandbox`, and `Samples`; shared documentation sits in `Assets/_Project/Docs` with living notes in `Docs/Works`.
- Automated tests mirror runtime layout in `Assets/_Project/Tests/<Domain>/<Editor|Runtime>`; Unity-generated outputs under root `Library/`, `Logs/`, and `Tests/Results/` are disposable and should stay untracked.

## Build, Test, and Development Commands
- Open the project with Unity 6000.0.42f1 via Hub or `& "C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe" -projectPath "%cd%"` to hydrate packages.
- Batch edit-mode regression pass: `& "<UnityPath>\Unity.exe" -projectPath "%cd%" -batchmode -quit -runTests -testPlatform EditMode -testResults Assets/_Project/Tests/Results/editmode-results.xml`.
- Add play-mode coverage by switching `-testPlatform PlayMode`; keep XML and log outputs inside `Assets/_Project/Tests/Results`.
- Run the Architecture Compliance scanner inside the editor (`Tools > Architecture > Compliance Checker`) and `pwsh -File backup_project.ps1` before invasive refactors to snapshot the working tree.

## Coding Style & Naming Conventions
- Follow the Allman brace style with four-space indents; align with `Assets/_Project/Docs/CODING_CONVENTIONS.md`.
- Root namespaces begin at `asterivo.Unity60.*`; features must not depend directly on other features, and templates never reach into `_Project.*`.
- Classes, enums, and constants use `PascalCase`; interfaces prefix `I`; private fields stay `_camelCase`; locals are `camelCase`.
- Prefer explicit types unless right-hand side is a constructor; expose serialized state with `[SerializeField] private` fields and document every public API with XML comments that explain intent.

## Testing Guidelines
- Place unit-level coverage for infrastructure in `Assets/_Project/Tests/Core/Editor`; gameplay behaviours belong in the corresponding `Features/<Feature>/Editor` or `Runtime` folders.
- Mirror namespace paths when naming fixtures and suffix files with `Tests`; avoid multi-responsibility suites.
- When submitting work, attach the relevant `Tests/Results/*.xml` or summary log and note any skipped suites; unexpected failures should be recorded in `Assets/_Project/Logs`.

## Commit & Pull Request Guidelines
- Keep commits single-purpose with concise titles (`Core: tighten ServiceLocator caching`); the existing history mixes English and Japanese - match the dominant style of the area you touch.
- Reference the related entry in `TASKS.md` or external ticket in the PR description, list the validation you performed, and include screenshots or short clips for visual updates.
- PRs should call out architecture impacts, new assets, and any manual setup required so downstream template consumers can reproduce the change.
- Before requesting review, ensure the compliance checker and at least one automated test pass are green; flag known gaps explicitly.

## Environment & Configuration Tips
- Unity version is pinned to `6000.0.42f1` with URP; install the Windows, Android, and iOS build modules if you serialize platform-specific assets.
- Keep `Packages/manifest.json` and `.asmdef` files authoritative; never hand-edit the generated `.csproj` files.
- Scripts assume Windows PowerShell (`pwsh`); if you contribute from macOS/Linux, mirror the directory casing and update only cross-platform-safe tooling.

