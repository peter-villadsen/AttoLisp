# AttoLisp

AttoLisp is a small Lisp interpreter written in C#. It was created to illustrate how easy it is to build a Lisp interpreter: the code prioritizes clarity and pedagogy over performance or completeness. The project includes a tokenizer, parser, evaluator, a small standard library (`stdlib.al`), and a unit test suite that demonstrates language features and correctness.

If you're learning about interpreters, AttoLisp is intentionally compact and readable so you can trace the evaluation flow and experiment by editing the source.

## Requirements

- .NET 9 SDK (or compatible)

## Build and run

From the repository root (PowerShell):

```powershell
# restore and build
dotnet restore
dotnet build

# run the program
dotnet run --project AttoLisp

# run the tests
dotnet test
```

## Project layout

- `AttoLisp/` - main project
- `AttoLisp.Tests/` - unit tests (xUnit)

## Creating a GitHub repository and pushing this project

Below are two exact, copy-paste-ready ways to create a GitHub repository for this project and push your code from PowerShell. Run these commands from the repository root `c:\Users\peter\OneDrive\Desktop\AttoLisp`. Replace `YourUserName` with your GitHub username or organization name.

Option 1 — Create the repository on GitHub (web UI)

1. Open https://github.com and sign in.
2. Click "New" (or the "+" → "New repository").
3. Name the repository `AttoLisp`, pick Public or Private, and do NOT initialize with a README (we already have one).
4. Create the repository. GitHub will show a repository URL such as `https://github.com/YourUserName/AttoLisp.git`.

Then run these PowerShell commands to push the code:

```powershell
git init
git add .
git commit -m "Initial commit — add .gitignore, README, LICENSE"
git branch -M main
git remote add origin https://github.com/YourUserName/AttoLisp.git
git push -u origin main
```

Option 2 — Create the repository using GitHub CLI (`gh`)

If you have the GitHub CLI installed and authenticated, you can create and push in one flow:

```powershell
git init
git add .
git commit -m "Initial commit — add .gitignore, README, LICENSE"
gh repo create YourUserName/AttoLisp --public --source=. --remote=origin --push
```

Notes

- To use SSH instead of HTTPS, replace the remote URL with the SSH URL GitHub shows (e.g. `git@github.com:YourUserName/AttoLisp.git`).
- If your local branch is named `master`, rename it to `main` (`git branch -M main`) or push the `master` branch instead.
- If Git prompts for credentials when pushing over HTTPS, use a Personal Access Token (PAT) or switch to SSH keys.

## License

This project is licensed under the MIT License — see the `LICENSE` file for details.

## CI status

![CI](https://github.com/peter-villadsen/AttoLisp/actions/workflows/dotnet-test.yml/badge.svg)

## Coverage

[![codecov](https://codecov.io/gh/peter-villadsen/AttoLisp/branch/main/graph/badge.svg)](https://codecov.io/gh/peter-villadsen/AttoLisp)

The workflow generates an HTML coverage report which is uploaded as an artifact on each run. To view the full HTML report:

1. Open the repository on GitHub: https://github.com/peter-villadsen/AttoLisp
2. Click the "Actions" tab and select the latest workflow run for the `.NET Tests` workflow.
3. On the workflow run page, expand the "Artifacts" section and download the `coverage-report` artifact — it contains a browsable HTML coverage report.
