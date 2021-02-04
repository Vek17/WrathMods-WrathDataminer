using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Kingdom;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Kingmaker.Visual.Critters;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Kingmaker.BundlesLoading;
using HarmonyLib;

namespace CustomBlueprints
{
    public class AssetsDump
    {
        public static BlueprintScriptableObject[] GetBlueprints()
        {
            var bundle = (AssetBundle)AccessTools.Field(typeof(ResourcesLibrary), "s_BlueprintsBundle")
                .GetValue(null);
            return bundle.LoadAllAssets<BlueprintScriptableObject>();
        }

        public static Dictionary<string, BlueprintScriptableObject> GetLoadedBlueprints()
        {
            return (Dictionary<string, BlueprintScriptableObject>)AccessTools
                .Field(typeof(ResourcesLibrary), "s_LoadedBlueprints")
                .GetValue(null);
        }
        public static Dictionary<string, BlueprintScriptableObject> GetBlueprintMap()
        {
            return GetBlueprints()
                .ToDictionary(bp => bp.AssetGuid, bp => bp);
        }
        public static Dictionary<string, string> GetResourceGuidMap()
        {
            var locationList = (LocationList)AccessTools
                .Field(typeof(BundlesLoadService), "m_LocationList")
                .GetValue(BundlesLoadService.Instance);
            return locationList.GuidToBundle;
        }
        public static void DumpBlueprint(BlueprintScriptableObject blueprint, string directory = "Blueprints", bool verbose = false)
        {
            JsonSerializerSettings settings = null;
            if (verbose)
            {
                settings = JsonBlueprints.CreateSettings();
                settings.DefaultValueHandling = DefaultValueHandling.Include;
            }
            JsonBlueprints.Dump(blueprint, $"{directory}/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json", settings);
        }
        public static void DumpBlueprints()
        {
            var seen = new HashSet<Type>();

            var blueprints = GetBlueprints();
            foreach (var blueprint in blueprints)
            {
                if (!seen.Contains(blueprint.GetType()))
                {
                    seen.Add(blueprint.GetType());
                    DumpBlueprint(blueprint);
                }
            }
        }
        public static void DumpScriptableObjects()
        {
            Directory.CreateDirectory("ScriptableObjects");
            foreach (var obj in UnityEngine.Object.FindObjectsOfType<ScriptableObject>())
            {
                try
                {
                    if (obj is BlueprintScriptableObject blueprint &&
                        !GetBlueprintMap().ContainsKey(blueprint.AssetGuid))
                    {
                        JsonBlueprints.Dump(blueprint, $"ScriptableObjects/{blueprint.GetType()}/{blueprint.name}.{blueprint.AssetGuid}.json");
                    }
                    else
                    {
                        JsonBlueprints.Dump(obj, $"ScriptableObjects/{obj.GetType()}/{obj.name}.{obj.GetInstanceID()}.json");
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllText($"ScriptableObjects/{obj.GetType()}/{obj.name}.{obj.GetInstanceID()}.txt", ex.ToString());
                }
            }
        }
        public static void DumpQuick()
        {
            var types = new HashSet<Type>()
            {
                typeof(BlueprintCharacterClass),
                typeof(BlueprintRaceVisualPreset),
                typeof(BlueprintRace),
                typeof(BlueprintArchetype),
                typeof(BlueprintProgression),
                typeof(BlueprintStatProgression),
                typeof(BlueprintFeature),
                typeof(BlueprintFeatureSelection),
                typeof(BlueprintSpellbook),
                typeof(BlueprintSpellList),
                typeof(BlueprintSpellsTable),
                typeof(BlueprintItemWeapon),
                typeof(BlueprintBuff)
            };
            foreach (var blueprint in GetBlueprints())
            {
                if (types.Contains(blueprint.GetType())) DumpBlueprint(blueprint);
            }
        }
        public static void DumpAllBlueprints()
        {
            var blueprints = GetBlueprints();
            Directory.CreateDirectory("Blueprints");
            using (var file = new StreamWriter("Blueprints/log.txt"))
            {
                foreach (var blueprint in blueprints)
                {
                    Main.DebugLog($"Dumping {blueprint.name} - {blueprint.AssetGuid}");
                    try
                    {
                        DumpBlueprint(blueprint);
                    }
                    catch (Exception ex)
                    {
                        file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                    }
                }
            }
        }
        public static void DumpAllBlueprintsVerbose()
        {
            var blueprints = GetBlueprints();
            Directory.CreateDirectory("BlueprintsVerbose");
            using (var file = new StreamWriter("BlueprintsVerbose/log.txt"))
            {
                foreach (var blueprint in blueprints)
                {
                    Main.DebugLog($"Dumping {blueprint.name} - {blueprint.AssetGuid}");
                    try
                    {
                        DumpBlueprint(blueprint, directory: "BlueprintsVerbose", verbose: true);
                    }
                    catch (Exception ex)
                    {
                        file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                    }
                }
            }
        }
        static void DumpResource(UnityEngine.Object resource, string assetId)
        {
            Directory.CreateDirectory($"Blueprints/{resource.GetType()}");
            JsonBlueprints.Dump(resource, $"Blueprints/{resource.GetType()}/{resource.name}.{assetId}.json");
        }
        public static void DumpEquipmentEntities()
        {
            foreach (var bp in GetBlueprints())
            {
                var resource = ResourcesLibrary.TryGetResource<EquipmentEntity>(bp.AssetGuid);
                if (resource == null) continue;
                DumpResource(resource, bp.AssetGuid);
                ResourcesLibrary.CleanupLoadedCache();
            }
        }
        public static void DumpUnitViews()
        {
            foreach (var bp in GetBlueprints())
            {
                var resource = ResourcesLibrary.TryGetResource<UnitEntityView>(bp.AssetGuid);
                if (resource == null) continue;
                DumpResource(resource, bp.AssetGuid);
                ResourcesLibrary.CleanupLoadedCache();
                //break;
            }
        }
        public static void DumpAssets()
        {
            DumpList();
            DumpAllBlueprints();
            DumpEquipmentEntities();
            DumpUnitViews();
        }
        public static void DumpList()
        {
            var resourceTypes = new Type[]
            {
                typeof(EquipmentEntity),
                typeof(Familiar),
                typeof(UnitEntityView),
                typeof(ProjectileView)
                //Note: PrefabLink : WeakResourceLink<GameObject> exists
            };
            Directory.CreateDirectory($"Blueprints/");
            var loadedBlueprints = GetLoadedBlueprints();
            var blueprints = GetBlueprints();
            Main.DebugLog($"LoadedBlueprints contains  {loadedBlueprints.Count} blueprints");
            Main.DebugLog($"Blueprint bundle contains  {blueprints.Length} blueprints");
            Main.DebugLog($"Dumping {blueprints.Length} blueprints");
            using (var file = new StreamWriter("Blueprints/Blueprints.txt"))
            {
                file.WriteLine($"name\tAssetId\tType");
                foreach (var blueprint in blueprints)
                {
                    file.WriteLine($"{blueprint.name}\t{blueprint.AssetGuid}\t{blueprint.GetType()}");
                }
            }
            var resourcePathsByAssetId = GetResourceGuidMap();
            Main.DebugLog($"ResourcePathsByAssetId contains {resourcePathsByAssetId.Count} resources");
            using (var file = new StreamWriter("Blueprints/Resources.txt"))
            {
                file.WriteLine($"Name\tAssetId\tBundleName\tType\tBaseType");
                foreach (var kv in resourcePathsByAssetId)
                {
                    var resource = ResourcesLibrary.TryGetResource<UnityEngine.Object>(kv.Key);
                    if (resource != null)
                    {
                        var baseType = resource.GetType().IsAssignableFrom(typeof(GameObject)) ? "GameObject" :
                                         resource.GetType().IsAssignableFrom(typeof(ScriptableObject)) ? "ScriptableObject" :
                                         resource.GetType().IsAssignableFrom(typeof(Component)) ? "Component" :
                                         "Object";
                        var go = resource as GameObject;
                        var typeName = resource?.GetType().Name ?? "NULL";
                        if (go != null)
                        {
                            foreach (var type in resourceTypes)
                            {
                                if (go.GetComponent(type) != null)
                                {
                                    typeName = type.Name;
                                }
                            }
                        }
                        file.WriteLine($"{resource.name}\t{kv.Key}\t{kv.Value}\t{typeName}\t{baseType}");
                        ResourcesLibrary.CleanupLoadedCache();
                    }
                }
            }
        }
        public static void DumpAssetBundles()
        {
            Directory.CreateDirectory($"Blueprints/");
            var bundles = Directory.GetFiles("Kingmaker_Data/StreamingAssets/Bundles")
                    .Select(f => Path.GetFileName(f));

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToDictionary(b => b.name);
            var file = new StreamWriter("Blueprints/AssetBundles.txt");
            file.WriteLine($"Loaded Bundles");
            foreach (var kv in loadedBundles)
            {
                file.WriteLine(kv.Key + $" IsNull {kv.Value == null}");
            }
            file.WriteLine("\nResourceBundles");
            foreach (var bundleNameExt in bundles)
            {
                if (!bundleNameExt.StartsWith("resource") || bundleNameExt.EndsWith("manifest")) continue;
                var bundleName = Path.GetFileNameWithoutExtension(bundleNameExt);
                string bundlePath = PathUtils.BundlePath(bundleName);
                var bundle = loadedBundles.ContainsKey(bundleName) ?
                    loadedBundles[bundleName] :
                    AssetBundle.LoadFromFile(bundlePath);
                file.WriteLine(bundleName);
                if (bundle == null)
                {
                    file.WriteLine($"  NULL, IsLoaded {loadedBundles.ContainsKey(bundlePath)}");
                    continue;
                }
                foreach (var name in bundle.GetAllAssetNames())
                {
                    file.WriteLine($"  Name: {name}");
                    var asset = bundle.LoadAsset(name);
                    file.WriteLine($"    Asset: {asset.name}, {asset.GetType().Name}");
                    foreach (var subAsset in bundle.LoadAssetWithSubAssets(name))
                    {
                        file.WriteLine($"    SubAsset: {subAsset}, {subAsset.GetType().Name}");
                    }
                }
                foreach (var scenePath in bundle.GetAllScenePaths())
                {
                    file.WriteLine($"  ScenePath: {scenePath}");
                }
                if (!loadedBundles.ContainsKey(bundle.name))
                {
                    bundle.Unload(true);
                }
            }
        }
        public static void DumpUI()
        {
            JsonBlueprints.DumpResource(Game.Instance, "UI/Game.json");
            JsonBlueprints.DumpResource(Game.Instance.UI, "UI/Game.UI.json");
            JsonBlueprints.DumpResource(Game.Instance.BlueprintRoot.UIRoot, "UI/Game.BlueprintRoot.UIRoot.json");
            JsonBlueprints.DumpResource(Game.Instance.DialogController, "UI/Game.DialogController.json");
            var ui = Game.Instance.UI;
            foreach (var field in ui.GetType().GetFields())
            {
                try
                {
                    var value = field.GetValue(ui);
                    if (value == null)
                    {
                        Main.DebugLog($"Null field {field.Name}");
                        continue;
                    }
                    JsonBlueprints.DumpResource(value, $"UI/UI.{value.GetType().FullName}.json");
                }
                catch (Exception ex)
                {
                    Main.DebugLog($"Error dumping UI field {field.Name}");
                }
            }
            foreach (var prop in ui.GetType().GetProperties())
            {
                try
                {
                    var value = prop.GetValue(ui);
                    if (value == null)
                    {
                        Main.DebugLog($"Null property {prop.Name}");
                        continue;
                    }
                    JsonBlueprints.DumpResource(value, $"UI/UI.{value.GetType().FullName}.json");
                }
                catch (Exception ex)
                {
                    Main.DebugLog($"Error dumping UI property {prop.Name}");
                }
            }
        }
        public static void DumpKingdom()
        {
            JsonBlueprints.Dump(KingdomState.Instance, "Kingdom");
        }
        public static void DumpSceneList()
        {
            string result = "";
            for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var scene = SceneManager.GetSceneByBuildIndex(i);
                result += $"{i}\t{scenePath}\t{scene.name}";
            }
            Directory.CreateDirectory("Dump");
            File.WriteAllText("Dump/SceneList.txt", result);
        }
    }
}
