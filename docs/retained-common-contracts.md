# Retained Common and boolean validation contracts

Issue: <https://github.com/MALIEV-Co-Ltd/Legacy.Maliev.CompatibilityContracts/issues/16>.

This slice preserves the original public CLR namespaces and source behavior for
`SocialNetworks`, `SupportedFileClass`, `Severity`, `TravelerSortType`, and
`BooleanRequiredAttribute`. The social properties remain static getters; the
format registries remain settable dictionaries with the complete existing groups
and extension ordering. The boolean attribute accepts both boolean values and
rejects null and non-boolean values. Its nullable annotation is adapted to .NET 10
without changing the runtime signature or validation semantics.

Source checkpoint: `135e526d0dab85c415b3afdcefd7b70fe2c82e2f`.

Source history retained by this slice:

| Full source SHA | Applicable behavior |
| --- | --- |
| `5fac706a7983a6d359b39acbd670e6800afe020e` | Initial Common and DataAnnotations types and documentation |
| `92fe441270b7ad0fcbf89a6ad9633a07a318b3d9` | Social destination changes |
| `4c4ba6050f824a04b3a21efbac6ef9ebc4634096` | Source formatting, no wire change |
| `ee9dc5ca48f4a5fe318e49c4592fe98cceb6cf22` | Verified social destinations and documentation |
| `96071102542087d6b6ffe49717bf26f0b4d6879c` | Merge of social destination changes |
| `2d291c25dd9740ed595ab328b046b5d716c29440` | WhatsApp destination and documentation |
| `03dc9a1271c16e6535934445e9dd6e3f30e8fffe` | Generated documentation belongs in build output |

The authoritative tracking repository retains individual source parents, complete
paths, owning services, and dispositions. This scoped record does not alter it.

Observed acceptance in the first finite validation slot: Release builds before
and after implementation each reported zero warnings and errors. All eleven new
runtime tests first failed because the required public types were absent; after
implementation all eleven passed, with no failures or skipped tests. Those tests
exercise reflection shape, concrete property values, enum JSON round trips,
registry setters and restore behavior, and DataAnnotations validation results.
Source copying and text searches are not used as behavior acceptance.

The reviewed local head passes twelve focused and all seventeen suite tests, with
no failures or skipped tests, after a zero-warning, zero-error Release build. The
documentation test first observed missing assembly XML, then passed after XML
generation was enabled. The package contains the assembly and its XML summaries.
Raw owned-assembly coverage measures 281 of 283 lines (99.29%) with no file,
attribute, or class exclusions and automatic property accessors included. The
required PR workflow runs these same coverage settings and rejects results below
80%. Format verification, transitive package vulnerability audit, and worktree and
Git-history secret scans pass. Joined consumer acceptance and exact-head PR and
post-merge main CI remain pending. No source obligation is marked complete by the
local result alone.

The modern Web service currently retains its own social constants under
`Legacy.Maliev.Web.Application`. This slice restores the original shared CLR
surface and does not change that consumer. Localization middleware is owned by
ServiceDefaults and must not be duplicated here. The source entity pagination
helper has an EF-backed asynchronous query method and is not reduced to a DTO or
message envelope. Its owner obligation, along with the removed source outage
notice lifecycle, needs separate evidence before the initial source commit can
be fully resolved. Existing CAD format entries are compatibility data; this slice
does not add CNC processing functionality.
