# Legacy.Maliev.CompatibilityContracts

Minimal .NET 10 wire contracts retained by extracted legacy MALIEV services.

The initial package intentionally preserves the existing
`Maliev.MessagingContracts.Contracts.*` CLR namespaces because those names are part
of MassTransit's message URN and therefore part of the runtime wire contract. The
repository, assembly, and package identities are independently named
`Legacy.Maliev.CompatibilityContracts` to avoid conflict with the new platform.

Initial source baseline:
`MALIEV-Co-Ltd/Maliev.MessagingContracts@c533c12a8154f5cf7c4fbc9734e82a62705ac60f`.

## Included surface

- shared `BaseMessage` envelope
- stable `MessageType` names and ordinals
- IAM `PermissionRegistrationRequest` and its permission/role item records
- retained `Maliev.Common` social destinations, file-format registry, and enumerations
- `Maliev.AspNetCore.DataAnnotations.BooleanRequiredAttribute`, which requires a
  boolean value and accepts both `true` and `false`

The retained Common and validation types keep their original CLR namespaces and
public accessors. Their migration source is `maliev-web` at
`135e526d0dab85c415b3afdcefd7b70fe2c82e2f`; see
[the scoped acceptance record](docs/retained-common-contracts.md).

Unrelated pricing, geometry, customer, notification, and other new-platform
contracts are deliberately excluded.

## Validation

```powershell
dotnet restore Legacy.Maliev.CompatibilityContracts.slnx
dotnet build Legacy.Maliev.CompatibilityContracts.slnx -c Release --no-restore
dotnet test Legacy.Maliev.CompatibilityContracts.slnx -c Release --no-build --no-restore
dotnet format Legacy.Maliev.CompatibilityContracts.slnx --verify-no-changes --no-restore
dotnet pack src/Legacy.Maliev.CompatibilityContracts/Legacy.Maliev.CompatibilityContracts.csproj -c Release --no-build
```

This repository creates no infrastructure and contains no credentials or production
data.
