# 🛠️ How To Develop Mods in This Repository

This repository contains multiple **Valheim mod projects** built using **BepInEx** and **SDK-style .NET projects**.  
All projects share a unified configuration through a `Directory.Build.props` file and common references to Valheim’s game assemblies.

Follow this guide to get your local environment ready for development.

---

## 🚀 1. Prerequisites

You’ll need the following installed:

- **Visual Studio 2022** (Community or higher)
  - Workload: `.NET desktop development`
  - Optional: Unity workload (for better code completion)
- **.NET SDK 4.8 Developer Pack** (if not included with Visual Studio)
- **Git** for cloning and version control
- **Valheim** installed locally (via Steam)

---

## 📂 2. Repository Setup

Clone this repository and open the solution file:

```bash
git clone https://github.com/jneb802/MoreWorldLocations_All.git
cd MoreWorldLocations_All
```

Do not yet open `MoreWorldLocations_All.sln`, there is more configuration to do.

---

## ⚙️ 3. Configure the Game Path

All projects use an environment property called `ValheimGamePath` that tells MSBuild where to find Valheim’s managed assemblies (the game DLLs).

1. **Copy the example file:**

   ```bash
   copy Directory.Build.props.example Directory.Build.props
   ```

2. **Edit `Directory.Build.props`:**

   Find this line:
   ```xml
   <ValheimGamePath>E:\SteamLibrary\steamapps\common\Valheim</ValheimGamePath>
   ```

   Replace it with your own Valheim installation directory, for example:
   ```xml
   <ValheimGamePath>Z:\SteamLibrary\steamapps\common\Valheim</ValheimGamePath>
   ```

   > 💡 You can find your Valheim install by right-clicking the game in Steam →  
   > **Manage → Browse local files.**

3. Save the file.  
   Visual Studio and MSBuild will automatically use this path to locate:
   - `valheim_Data\Managed\assembly_valheim.dll`
   - `BepInEx\core\BepInEx.dll`
   - `UnityEngine` DLLs, etc.

---

## 📦 4. Restore NuGet Packages

The solution depends on several NuGet feeds:

- **nuget.org** (for standard .NET packages)
- **BepInEx feed** (for publicizer and modding tasks)

If you’ve never built before, restore all dependencies via either:

### Option A — Visual Studio
1. Open **Solution Explorer**
2. Right-click the solution → **Restore NuGet Packages**

### Option B — Command Line
```bash
dotnet restore
```

Make sure these feeds exist in your NuGet sources:
```bash
dotnet nuget list source
```
Expected entries:
```
nuget.org                https://api.nuget.org/v3/index.json
BepInEx                  https://nuget.bepinex.dev/v3/index.json
```

If they don’t exist, add them:
```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet nuget add source https://nuget.bepinex.dev/v3/index.json -n BepInEx
```

---

## 🧩 5. Build the Solution

Now you can build all projects normally:

- From Visual Studio: **Build → Build Solution (Ctrl + Shift + B)**
- From CLI:
  ```bash
  dotnet build -c Debug
  ```

Each project compiles to:
```
<ProjectFolder>\bin\<Configuration>\net48\<ProjectName>.dll
```

---

## 🧰 6. BepInEx Assembly Publicizer

The solution includes the **BepInEx.AssemblyPublicizer.MSBuild** package.  
This automatically “publicizes” Valheim’s assemblies (e.g. `assembly_valheim.dll`, `assembly_utils.dll`) during build time so your mods can access internal types and methods.

You don’t need to run the CLI manually — it’s fully integrated through MSBuild.

---

## 🧱 7. Common Project Setup

All mod projects share code through a `Common` project (SDK-style).  
Any shared utilities or extensions live there and are referenced via:

```xml
<ProjectReference Include="..\Common\Common.csproj" />
```

This ensures all mods compile against the same base functionality.

---

## 🔄 8. Output Copy & Packaging

When you build in **Release** configuration:
- The DLLs will be copied automatically to any configured `CopyOutputDLLPath` folders (if set).
- The custom build targets also prepare Thunderstore/Nexus mod packages automatically if you use the included packaging scripts.

---

## ✅ Summary

| Step | Task | Description |
|------|------|--------------|
| 1 | Prerequisites | Install VS2022 + .NET 4.8 SDK |
| 2 | Clone repo | `git clone` the repository |
| 3 | Configure path | Copy and edit `Directory.Build.props` |
| 4 | Restore packages | Run `dotnet restore` |
| 5 | Build | `dotnet build -c Debug` or build via Visual Studio |
| 6 | Test | Launch Valheim with your mod DLL in `BepInEx/plugins` |
| 7 | Package | Build in Release to auto-create Thunderstore/Nexus zips |

---

Happy modding! 🧙‍♂️
