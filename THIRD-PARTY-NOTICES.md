# Third-party notices

**Let Kabale (EasySolitaire)** is © 2026 Per Hejndorf and is licensed under the [MIT License](LICENSE), except where stated below. This file lists the third-party components and assets it uses, each under its own licence. Licence texts and copyright notices are kept by the upstream projects at the *Source* links; where a licence requires its notice to travel with binaries, the upstream package already carries it.

_Last reviewed: 2026-11-22. Versions are the ones resolved in this repository at that date; Dependabot keeps them current, so check the project files for the exact version of a given release._

## Libraries included in the application

| Library | Version | Licence | Source | Modified |
|---|---|---|---|---|
| .NET runtime and Blazor WebAssembly (shipped in `_framework/`) | 10.0 | MIT | [dotnet/runtime](https://github.com/dotnet/runtime), [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | No |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly/10.0.15) | No |

<details>
<summary>Indirect (transitive) dependencies — 25 packages</summary>

These packages are pulled in by the libraries above and are shipped with the application. All are used unmodified.

| Library | Version | Licence | Source | Modified |
|---|---|---|---|---|
| Microsoft.AspNetCore.Authorization | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Authorization/10.0.15) | No |
| Microsoft.AspNetCore.Components | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Components/10.0.15) | No |
| Microsoft.AspNetCore.Components.Forms | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.Forms/10.0.15) | No |
| Microsoft.AspNetCore.Components.Web | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.Web/10.0.15) | No |
| Microsoft.AspNetCore.Metadata | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Metadata/10.0.15) | No |
| Microsoft.Extensions.Configuration | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/10.0.15) | No |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Abstractions/10.0.15) | No |
| Microsoft.Extensions.Configuration.Binder | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder/10.0.15) | No |
| Microsoft.Extensions.Configuration.FileExtensions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.FileExtensions/10.0.15) | No |
| Microsoft.Extensions.Configuration.Json | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/10.0.15) | No |
| Microsoft.Extensions.DependencyInjection | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/10.0.15) | No |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/10.0.15) | No |
| Microsoft.Extensions.Diagnostics | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics/10.0.15) | No |
| Microsoft.Extensions.Diagnostics.Abstractions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.Abstractions/10.0.15) | No |
| Microsoft.Extensions.FileProviders.Abstractions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.FileProviders.Abstractions/10.0.15) | No |
| Microsoft.Extensions.FileProviders.Physical | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.FileProviders.Physical/10.0.15) | No |
| Microsoft.Extensions.FileSystemGlobbing | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing/10.0.15) | No |
| Microsoft.Extensions.Logging | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Logging/10.0.15) | No |
| Microsoft.Extensions.Logging.Abstractions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/10.0.15) | No |
| Microsoft.Extensions.Options | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Options/10.0.15) | No |
| Microsoft.Extensions.Options.ConfigurationExtensions | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Options.ConfigurationExtensions/10.0.15) | No |
| Microsoft.Extensions.Primitives | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Primitives/10.0.15) | No |
| Microsoft.Extensions.Validation | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Validation/10.0.15) | No |
| Microsoft.JSInterop | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.JSInterop/10.0.15) | No |
| Microsoft.JSInterop.WebAssembly | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.JSInterop.WebAssembly/10.0.15) | No |

</details>

## Build, test and tooling (not distributed)

Used to build, test or render promotional material only; not part of the shipped application.

| Library | Version | Licence | Source | Modified |
|---|---|---|---|---|
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 10.0.15 | MIT | [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebAssembly.DevServer/10.0.15) | No |
| Microsoft Edge (headless), used by `tools/make-icons.ps1` to render `art/icon.svg` | — | Microsoft software licence terms | [microsoft.com/edge](https://www.microsoft.com/edge) | No |

## Icons, images, audio and video

| Asset(s) | Origin | Licence |
|---|---|---|
| `art/icon.svg`, `src/EasySolitaire/wwwroot/icons/` (8 files: .png, rendered from `art/icon.svg` by `tools/make-icons.ps1`) | Drawn by the author as SVG (written with an AI coding agent), not made with an image-generation tool. The cat on the front card is the 🐱 emoji in Microsoft's Fluent Emoji design, rendered from the Segoe UI Emoji font | MIT (as part of this project); the cat emoji design is © Microsoft, MIT ([microsoft/fluentui-emoji](https://github.com/microsoft/fluentui-emoji)) |
| `src/EasySolitaire/wwwroot/img/card-back.svg` (the back of the cards) | Drawn by the author as SVG (written with an AI coding agent), not made with an image-generation tool | MIT (as part of this project) |
| `images/banner.png` | Screenshot of the game | MIT (as part of this project) |
| Card symbols and animals in the game (♥ ♦ ♠ ♣, 🐱 🐶 🐼 🐸 and other emoji) | Not shipped: drawn at run time by the fonts installed on the player's device | — |

The game has no sound, music or video.

## Trademarks

Microsoft, Windows, .NET, Blazor and other Microsoft product names are trademarks of the Microsoft group of companies. All other product names, logos and brands mentioned in this repository are the property of their respective owners. They are used only to identify the services and tools this project works with, and their use does not imply endorsement.
