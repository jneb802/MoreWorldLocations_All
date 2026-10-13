using System;
using System.Linq;
using BepInEx.Bootstrap;
using More_World_Locations_AIO;
using More_World_Locations_AIO.Utils;

bool fortressLoaded = args.Single() == "present";
if (fortressLoaded) Chainloader.PluginInfos.Add(More_World_Locations_AIOPlugin.ValheimFortressGUID, new object());
else Chainloader.PluginInfos.Add("unrelated.plugin", new object());

MWLLocation shrine = LocationDefinitions.Plains.Single(location => location.Name == "MWL_ShrineEvent1");
MWLLocation minddrip = LocationDefinitions.Plains.Single(location => location.Name == "MWL_MinddripHallow1");
Check(shrine.IsAvailable == fortressLoaded, "dependency availability");
Check(shrine.RequiredPlugin == "MidnightsFX.ValheimFortress", "exact Fortress plugin GUID");
Check((LocationDB.GetLocation(shrine.Name) != null) == fortressLoaded, "name lookup");
Check((LocationDB.GetLocationConfig(shrine.Name) != null) == fortressLoaded, "config lookup");
Check(LocationDB.GetAllAssetPaths().Contains(shrine.AssetPath) == fortressLoaded, "asset paths");
Check(LocationDB.GetLocationNames(Heightmap.Biome.Plains).Contains(shrine.Name) == fortressLoaded, "biome names");
Check(LocationDB.GetLocation(minddrip.Name) == minddrip, "existing location remains available");
foreach (System.Reflection.PropertyInfo property in typeof(Jotunn.Configs.LocationConfig).GetProperties())
    Check(Equals(property.GetValue(shrine.Config), property.GetValue(minddrip.Config)), "Minddrip default: " + property.Name);

shrine.Register();
Check(Common.LocationManager.Registered.Contains(shrine.Name) == fortressLoaded, "direct registration");
Common.LocationManager.Registered.Clear();
LocationDB.RegisterAll();
Check(Common.LocationManager.Registered.Contains(shrine.Name) == fortressLoaded, "bulk registration");
Check(Common.LocationManager.Registered.Count(name => name == minddrip.Name) == 1, "existing location registers once");
Check(ProtectedLocationPeacefulArea.Patched.Contains(shrine.Name) == fortressLoaded, "no prefab patch without dependency");

UpgradeWorldCommands.AddUpgradeWorldCommands();
foreach (string command in new[] { "mwl_allbiomes", "mwl_plains" })
{
    Check(UpgradeWorld.Upgrade.Commands[command].Contains(shrine.Name) == fortressLoaded, command);
    Check(UpgradeWorld.Upgrade.Commands[command].Contains(minddrip.Name), command + " retains Minddrip");
}
Console.WriteLine("PASS: Fortress " + (fortressLoaded ? "present" : "absent"));

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAIL: " + message);
}
