# Branching strategy — GitFlow

## Long-lived branches

| Branch    | Role                                                                 | Rules |
| --------- | ------------------------------------------------------------------- | ----- |
| `master`  | Production. Every commit is a tagged release (`vX.Y.Z`).            | No direct commits. Updated only by merging `release/*` or `hotfix/*`. Protected. |
| `develop` | Integration branch. The **default branch** — PRs target it.         | No direct commits. Updated only by merging PRs. Protected. |

## Short-lived branches

| Prefix       | Cut from  | Merges back into        | Purpose |
| ------------ | --------- | ----------------------- | ------- |
| `feature/*`  | `develop` | `develop`               | New functionality. e.g. `feature/checkin-qr`, `feature/payments-list`. |
| `bugfix/*`   | `develop` | `develop`               | Non-urgent fixes for something already on `develop` (not yet released). |
| `release/*`  | `develop` | `master` **and** `develop` | Stabilise a version. Only version bumps, changelog, and blocker fixes. e.g. `release/1.2.0`. |
| `hotfix/*`   | `master`  | `master` **and** `develop` | Urgent production fix. e.g. `hotfix/1.2.1`. |

## Flow

```
feature/x ──┐
feature/y ──┼──▶ develop ──▶ release/1.2.0 ──▶ master  (tag v1.2.0)
            │                      └──────────────▶ develop   (merge back)
            │
hotfix/1.2.1 ◀── master ──▶ master  (tag v1.2.1)
             └───────────────────▶ develop   (merge back)
```

## Merge & commit rules

- **Commits and PR titles follow Conventional Commits** (`type(scope): summary`) — see `dotnet-api` / repo conventions.
- `feature/*` → `develop`: **squash-merge**. The squash commit's subject is the Conventional Commit summary.
- `release/*` and `hotfix/*` merges: **merge commit (`--no-ff`)** into both `master` and `develop`, so the release point stays visible in history.
- Every merge into `master` is immediately tagged: `git tag -a vX.Y.Z -m "vX.Y.Z"` and `git push --tags`.
- Versioning is **SemVer**. The version is bumped inside the `release/*` (or `hotfix/*`) branch.
- Keep branches short-lived; rebase on the latest `develop` before opening the PR.

## Typical commands

```bash
# feature
git switch develop && git pull
git switch -c feature/payments-list
# …work…
git push -u origin feature/payments-list        # open PR → develop (squash)

# release
git switch -c release/1.2.0 develop
# bump version, update CHANGELOG, fix blockers only
git push -u origin release/1.2.0                # open PR → master
# after merge to master:
git switch master && git pull
git tag -a v1.2.0 -m "v1.2.0" && git push --tags
git switch develop && git merge --no-ff master && git push   # merge back

# hotfix
git switch -c hotfix/1.2.1 master
# …fix…  bump patch version
git push -u origin hotfix/1.2.1                 # PR → master, then tag, then merge back to develop
```

## Optional: git-flow (AVH) extension

`git flow init` answers for this repo:

```
Production branch:   master
Development branch:  develop
Feature prefix:      feature/
Bugfix prefix:       bugfix/
Release prefix:      release/
Hotfix prefix:       hotfix/
Support prefix:      support/
Version tag prefix:  v
```

## Branch protection (set on GitHub)

For `master` and `develop`:

- Require a pull request before merging (≥1 approval).
- Require status checks to pass (build + `dotnet test`).
- Require branches to be up to date before merging.
- Disallow force pushes and deletions.
