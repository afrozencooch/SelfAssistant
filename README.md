# SelfAssistant

[![.NET](https://github.com/afrozencooch/SimpleDotnetApp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/afrozencooch/SimpleDotnetApp/actions)

This repository now contains a minimal backend service (`SelfAssistant.Service`) and a simple WinForms GUI (`SelfAssistant.Gui`) to demonstrate a basic chat flow.

Design notes & constraints:
- Windows Services run in Session 0 and cannot display interactive UI directly. For that reason this project separates the backend (runs as a service) from the GUI (a WinForms client) which communicates with the service over HTTP on localhost.

Quickstart (run locally for development):

PowerShell (start service host in foreground):
```powershell
cd .\Service
dotnet run
```

Run the GUI (requires Windows):
```powershell
cd ..\Gui
dotnet run
```

Install the backend as a Windows Service (production):
- Publish the Service and register it with `sc.exe` or use `New-Service`/`nssm`. The service listens on `http://localhost:5005` by default.

Notes:
- The GUI polls `GET /chat` and sends user input to `POST /chat`.
- This is a minimal scaffold; next steps would be to add message persistence, authentication, and an actual LLM backend.

## What I created

- `SimpleDotnetApp.csproj` — project file targeting `net8.0`.
- `Program.cs` — tiny console app demonstrating runtime info and args.
- `.gitignore` — common items to ignore for .NET projects.

## Quick start (Windows PowerShell)

```powershell
cd SimpleDotnetApp
# build and run
dotnet build
dotnet run -- arg1 arg2
```

## Initialize git and push to GitHub

Option A: using GitHub CLI (`gh`):

```powershell
cd SimpleDotnetApp
git init
git add .
git commit -m "Initial commit: scaffold SimpleDotnetApp"
# replace YOUR-REPO-NAME with desired repo name
gh repo create YOUR-REPO-NAME --public --source . --remote origin --push
```

Option B: manual via GitHub web UI:

```powershell
cd SimpleDotnetApp
git init
git add .
git commit -m "Initial commit: scaffold SimpleDotnetApp"
# Create a new repo on GitHub.com (no README). Then follow the instructions to add remote and push:
# git remote add origin https://github.com/USERNAME/REPO.git
# git branch -M main
# git push -u origin main
```

## Notes

- If you don't have the .NET SDK installed, download it from https://dotnet.microsoft.com.
- This scaffold targets `net8.0`; change `TargetFramework` in the `.csproj` if you prefer a different version.
