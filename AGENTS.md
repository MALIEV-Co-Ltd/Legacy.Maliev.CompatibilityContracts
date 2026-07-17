# Legacy.Maliev.CompatibilityContracts

This public .NET 10 repository owns only the message contracts required to keep
extracted legacy MALIEV services wire-compatible.

## Non-negotiable boundaries

- Preserve retained CLR full names, JSON property names, property types/order,
  constructors, enum names/ordinals, and MassTransit message URNs.
- Keep repository, assembly, and package identity `Legacy.Maliev.CompatibilityContracts`.
- Do not copy unrelated new-platform contracts merely for convenience.
- Contract changes require red-first reflection, JSON round-trip, and message-identity tests.
- Never add credentials, production payloads, customer data, or private migration evidence.
- This package creates no infrastructure and must not alter databases, deployments,
  Kubernetes, secrets, node pools, Cloud SQL, or paid resources.

## Required validation

```powershell
dotnet restore Legacy.Maliev.CompatibilityContracts.slnx
dotnet build Legacy.Maliev.CompatibilityContracts.slnx -c Release --no-restore
dotnet test Legacy.Maliev.CompatibilityContracts.slnx -c Release --no-build --no-restore
dotnet format Legacy.Maliev.CompatibilityContracts.slnx --verify-no-changes --no-restore
dotnet list Legacy.Maliev.CompatibilityContracts.slnx package --vulnerable --include-transitive --no-restore
dotnet pack src/Legacy.Maliev.CompatibilityContracts/Legacy.Maliev.CompatibilityContracts.csproj -c Release --no-build
gitleaks git . --redact=100 --exit-code 0 --no-banner --no-color
```
