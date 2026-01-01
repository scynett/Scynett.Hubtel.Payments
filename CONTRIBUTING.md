# Contributing

Thanks for helping improve **Scynett.Hubtel.Payments**! This project welcomes issues, pull requests, and feedback of all shapes and sizes. Please review this guide before submitting changes.

## Development workflow

1. Fork the repository and create a feature branch from `main`.
2. Keep changes focused; one feature or fix per pull request.
3. Ensure code builds and tests pass:
   ```bash
   dotnet build -c Release
   dotnet test tests/Scynett.Hubtel.Payments.Tests/Scynett.Hubtel.Payments.Tests.csproj -c Release
   ```
4. Adhere to the existing coding style (nullable enabled, analyzers ON, Activity instrumentation where appropriate).
5. Update documentation and samples when behavior changes.

## Releasing

The SDK follows **Semantic Versioning (SemVer)** with **tag-driven releases**. CI/CD jobs (coming soon) will pack and push NuGet packages whenever a valid tag is pushed. Until then, these manual steps keep releases consistent:

### 1. Decide the version bump

- **PATCH (`vX.Y.Z` → `vX.Y.(Z+1)`):** backwards-compatible bug fixes or doc-only updates.
- **MINOR (`vX.Y.Z` → `vX.(Y+1).0`):** backwards-compatible features, new APIs, or configuration additions.
- **MAJOR (`vX.Y.Z` → `v(X+1).0.0`):** breaking API changes or behavioral changes that require consumers to act.

### 2. Create an annotated tag

> **Tag format:** must start with `v` and follow SemVer, e.g. `v1.4.0` or `v1.4.0-rc.1`.

- Stable release:
  ```bash
  git tag -a vX.Y.Z -m "Release vX.Y.Z"
  ```
- Pre-release (e.g., release candidate):
  ```bash
  git tag -a vX.Y.Z-rc.1 -m "Release vX.Y.Z-rc.1"
  ```

### 3. Push the tag to origin

```bash
git push origin vX.Y.Z
```

*(Adjust the tag name for pre-releases as needed.)*

### 4. Draft GitHub release notes

1. Go to **GitHub → Releases → Draft a new release**.
2. Select the newly pushed tag.
3. Summarize notable changes (features, fixes, breaking changes, docs).
4. Publish the release. CI/CD will detect the tag and publish packages automatically (once the workflow is in place).

## Verification checklist

Before tagging a release, run the following commands and confirm they succeed:

```bash
dotnet build -c Release
dotnet test tests/Scynett.Hubtel.Payments.Tests/Scynett.Hubtel.Payments.Tests.csproj -c Release
dotnet pack -c Release
dotnet pack -c Release -p:PackageVersion=1.2.3
```

After `dotnet pack`, ensure the generated `.nupkg` files use the expected version (either the default `0.1.0-local` or the explicit value passed via `PackageVersion`).

Happy shipping! 🚀
