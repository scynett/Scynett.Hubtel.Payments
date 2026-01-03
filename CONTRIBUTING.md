# Contributing

Thanks for helping improve **Scynett.Hubtel.Payments**! This project welcomes issues, pull requests, and feedback of all shapes and sizes. Please review this guide before submitting changes.

## Development workflow

1. Fork the repository and create a feature branch from `main`.
2. Keep changes focused; one feature or fix per pull request.
3. Ensure code builds and tests pass locally:
   ```bash
   dotnet build Scynett.Hubtel.Payments.sln -c Release
   dotnet test Scynett.Hubtel.Payments.sln -c Release --logger "trx;LogFileName=test-results.trx"
   ```
4. Adhere to the existing coding style (nullable enabled, analyzers ON, Activity instrumentation where appropriate).
5. Update documentation and samples when behavior changes.

## Commit & PR guidelines

- **PR titles MUST follow [Conventional Commits](https://www.conventionalcommits.org/) because the repository uses squash merges.** The PR title becomes the commit message that release-please and the automation rely on for changelog generation and version bumps.
- Use this cheat-sheet to pick the correct prefix:

  | Change type | Prefix | Example |
  |-------------|--------|---------|
  | Patch       | `fix:` | `fix(hubtel): correct callback url mapping` |
  | Minor       | `feat:` | `feat(storage): add postgres pending transaction store` |
  | Major       | `feat!:` or add `BREAKING CHANGE` in the body | `feat!: change public payment gateway API` |

- Include a scope when it adds clarity, e.g., `feat(aspnetcore): add middleware`.
- Tests must pass (`dotnet test`) and CI must be green before requesting review.

## Automated releases

- Versioning is centralized in `Directory.Build.props` and tracked by release-please. The baseline version is **0.1.10** and all NuGet packages share the same SemVer.
- release-please opens or updates release PRs. Merging the release PR into `main` automatically:
  1. Cuts the tag `vX.Y.Z`.
  2. Creates the GitHub Release.
  3. Builds, packs, and publishes the NuGet packages (`ScynettPayments`, `ScynettPayments.AspNetCore`, `ScynettPayments.Storage.PostgreSql`) with the shared version.
- No manual tagging or publishing is needed. Land changes via PRs with correct titles and let the automation handle the rest.

## Verification checklist

Run the following commands locally (Release configuration) before submitting or merging a PR:

```bash
dotnet restore Scynett.Hubtel.Payments.sln
dotnet build Scynett.Hubtel.Payments.sln -c Release --no-restore
dotnet test Scynett.Hubtel.Payments.sln -c Release --no-build
```

Happy shipping!
