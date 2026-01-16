# SimpleDotnetApp

A minimal .NET console application scaffold targeting the latest stable .NET (net8.0).

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
