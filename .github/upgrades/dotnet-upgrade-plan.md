# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade NRobot.Server.Exceptions\NRobot.Server.Exceptions.csproj
4. Upgrade third-party\XmlRpc\Libraries\Horizon.XmlRpc.Core\Horizon.XmlRpc.Core.csproj
5. Upgrade NRobot.Server.Imp\NRobot.Server.Imp.csproj
6. Upgrade third-party\XmlRpc\Libraries\Horizon.XmlRpc.Server\Horizon.XmlRpc.Server.csproj
7. Upgrade NRobot.Server\NRobot.Server.csproj
8. Upgrade NRobot.Server.Test\NRobot.Server.Test.csproj


## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|
|                                                |                            |

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                                   |
|:------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| xmlrpcnet                            |     2.5.0       |             | No supported version found for .NET 9.0; replace or remove and use a maintained alternative |


### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### NRobot.Server.Exceptions\NRobot.Server.Exceptions.csproj modifications

Project properties changes:
  - Convert project file to SDK-style project format.
  - Target framework should be changed from `net48` (NET Framework 4.8) to `net9.0`.

NuGet packages changes:
  - No NuGet package changes detected for this project in analysis.

Other changes:
  - Update any assembly binding redirects and replace unsupported APIs as needed.

#### third-party\XmlRpc\Libraries\Horizon.XmlRpc.Core\Horizon.XmlRpc.Core.csproj modifications

Project properties changes:
  - Keep existing multi-targeting `netstandard2.0;netstandard2.1` and add `net9.0` to target frameworks: `netstandard2.0;netstandard2.1;net9.0`.
  - Ensure project is SDK-style (if not already) to support multi-targeting.

NuGet packages changes:
  - No direct NuGet package changes detected for this project in analysis.

Other changes:
  - Verify third-party library source compiles under `net9.0` and update any obsolete APIs.

#### NRobot.Server.Imp\NRobot.Server.Imp.csproj modifications

Project properties changes:
  - Convert project file to SDK-style project format.
  - Target framework should be changed from `net48` to `net9.0`.

NuGet packages changes:
  - `xmlrpcnet` (2.5.0) is incompatible with .NET 9.0 and no supported version was found. Replace `xmlrpcnet` with a maintained XmlRpc library compatible with .NET 9 (or remove and implement an alternative).

Feature upgrades:
  - Replace usages of `XmlRpc` library APIs with chosen replacement; adjust `XmlRpcListenerService`/`XmlRpcHttpServerProtocol` usages if replacement API differs.

Other changes:
  - Review `using System.Net;` usages and APIs that may have evolved; ensure `HttpListener` usage is supported on target platform (on net9.0 it may require `net7+` compatibility or changes for cross-platform hosting).

#### third-party\XmlRpc\Libraries\Horizon.XmlRpc.Server\Horizon.XmlRpc.Server.csproj modifications

Project properties changes:
  - Keep existing multi-targeting `netstandard2.0;netstandard2.1` and add `net9.0` to target frameworks: `netstandard2.0;netstandard2.1;net9.0`.
  - Ensure project is SDK-style.

NuGet packages changes:
  - No direct NuGet package changes detected for this project in analysis.

Other changes:
  - Verify server-side XmlRpc APIs compile under `net9.0` and adapt as needed.

#### NRobot.Server\NRobot.Server.csproj modifications

{