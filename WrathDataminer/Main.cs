using UnityEngine;
using UnityModManagerNet;
using System;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Blueprints.CharGen;
using System.IO;
using Kingmaker.DialogSystem.Blueprints;
using System.Linq;
using Kingmaker.Utility;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using System.Reflection;

namespace CustomBlueprints
{
#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        public static ILogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            if(logger != null) logger.Log(msg);
        }
        public static bool enabled;
        public static Settings settings;
        public static string ModPath = null;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
                //var harmony = HarmonyInstance.Create(modEntry.Info.Id);
                //harmony.PatchAll(Assembly.GetExecutingAssembly());
                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
#if DEBUG
                modEntry.OnUnload = Unload;
#endif
                ModPath = modEntry.Path;
                logger = new UMMLogger(modEntry.Logger);

            }
            catch (Exception e){
                modEntry.Logger.Log(e.ToString() +"\n" + e.StackTrace);
            }
            return true;
        }
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            //HarmonyInstance.Create(modEntry.Info.Id).UnpatchAll();
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        static UnityEngine.Object FindObject2(int instanceId)
        {
            return null; //can't find FindObjectFromInstanceID 

        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                if (!enabled) return;
#if (DEBUG)
                if (GUILayout.Button("DumpAssets"))
                {
                    AssetsDump.DumpAssets();
                }
                if (GUILayout.Button("DumpCompanions"))
                {
                    var blueprints = AssetsDump.GetBlueprints()
                        .OfType<BlueprintUnit>()
                        .Where(bp => bp.name.ToLower().Contains("companion"));
                    foreach (var blueprint in blueprints)
                    {
                        JsonBlueprints.Dump(blueprint, $"Companions/{blueprint.name}.{blueprint.AssetGuid}.json");
                    }
                }
                if (GUILayout.Button("DumpClassRaceBlueprints"))
                {
                    AssetsDump.DumpQuick();
                }
                if (GUILayout.Button("DumpSampleOfBlueprints")){
                    AssetsDump.DumpBlueprints();
                }
                if (GUILayout.Button("DumpAllBlueprints"))
                {
                    AssetsDump.DumpAllBlueprints();
                }
                if (GUILayout.Button("DumpFlags"))
                {
                    var blueprints = AssetsDump.GetBlueprints()
                        .OfType<BlueprintUnlockableFlag>();
                    Directory.CreateDirectory("Blueprints");
                    using (var file = new StreamWriter("Blueprints/log.txt"))
                    {
                        foreach (var blueprint in blueprints)
                        {
                            if (blueprint.AssetGuid.ToString().Length != 32) continue;
                            Main.DebugLog($"Dumping {blueprint.name} - {blueprint.AssetGuid}");
                            try
                            {
                                AssetsDump.DumpBlueprint(blueprint);
                            }
                            catch (Exception ex)
                            {
                                file.WriteLine($"Error dumping {blueprint.name}:{blueprint.AssetGuid}:{blueprint.GetType().FullName}, {ex.ToString()}");
                            }
                        }
                    }
                }
                if (GUILayout.Button("DumpEquipmentEntities"))
                {
                    AssetsDump.DumpEquipmentEntities();
                }
                if (GUILayout.Button("DumpUnitViews"))
                {
                    AssetsDump.DumpUnitViews();
                }
                if (GUILayout.Button("DumpList"))
                {
                    AssetsDump.DumpList();
                }
                if (GUILayout.Button("DumpScriptableObjects"))
                {
                    AssetsDump.DumpScriptableObjects();
                }
                if (GUILayout.Button("DumpAssetBundles"))
                {
                    AssetsDump.DumpAssetBundles();
                }
                if (GUILayout.Button("DumpUI"))
                {
                    AssetsDump.DumpUI();
                }
                if (GUILayout.Button("DumpSceneList"))
                {
                    AssetsDump.DumpSceneList();
                }
                if (GUILayout.Button("DumpKingdom"))
                {
                    AssetsDump.DumpKingdom();
                }
                if (GUILayout.Button("DumpView"))
                {
                    var view = ResourcesLibrary.TryGetResource<GameObject>("adf003833b2463543a065d5160c7e8f1");
                    var character = view.GetComponent<Character>();
                    JsonBlueprints.Dump(character, "adf003833b2463543a065d5160c7e8f1");
                }
                if (GUILayout.Button("TestLoad"))
                {
                    var vp = JsonBlueprints.Load<BlueprintRaceVisualPreset>("mods/customraces/data/TestPreset.json");
                    DebugLog("Loaded " + vp.name);
                }

                if (GUILayout.Button("Reload"))
                {

                }
#endif
            } catch(Exception e)
            {
                DebugLog(e.ToString() + " " + e.StackTrace);
            }
        }
#if false
		[HarmonyPatch(typeof(ReflectionBasedSerializer), "ReadField", new Type[] { typeof(FieldInfo), typeof(object) })]
		static class ReflectionBasedSerializer_Patch {
			static bool Prefix(ReflectionBasedSerializer __instance, FieldInfo field, object obj) {
				if (field.FieldType == typeof(GUIStyle)) {
					field.SetValue(obj, new GUIStyle());
					return false;
				}
				Type fieldType = field.FieldType;
				if (fieldType == typeof(int)) {
					int num = 0;
					__instance.m_Primitive.Int(ref num);
					field.SetValue(obj, num);
					return false;
				}
				if (fieldType == typeof(uint)) {
					uint num2 = 0U;
					__instance.m_Primitive.UInt(ref num2);
					field.SetValue(obj, num2);
					return false;
				}
				if (fieldType == typeof(float)) {
					float num3 = 0f;
					__instance.m_Primitive.Float(ref num3);
					field.SetValue(obj, num3);
					return false;
				}
				if (fieldType == typeof(double)) {
					double num4 = 0.0;
					__instance.m_Primitive.Double(ref num4);
					field.SetValue(obj, num4);
					return false;
				}
				if (fieldType == typeof(long)) {
					long num5 = 0L;
					__instance.m_Primitive.Long(ref num5);
					field.SetValue(obj, num5);
					return false;
				}
				if (fieldType == typeof(ulong)) {
					ulong num6 = 0UL;
					__instance.m_Primitive.ULong(ref num6);
					field.SetValue(obj, num6);
					return false;
				}
				if (fieldType == typeof(string)) {
					string value = null;
					__instance.m_Primitive.String(ref value);
					field.SetValue(obj, value);
					return false;
				}
				if (fieldType == typeof(bool)) {
					bool flag = false;
					__instance.m_Primitive.Bool(ref flag);
					field.SetValue(obj, flag);
					return false;
				}
				if (fieldType.IsEnum) {
					int value2 = 0;
					__instance.m_Primitive.Int(ref value2);
					field.SetValue(obj, Enum.ToObject(fieldType, value2));
					return false;
				}
				if (fieldType == typeof(Color)) {
					Color color = default(Color);
					__instance.m_Primitive.Color(ref color);
					field.SetValue(obj, color);
					return false;
				}
				if (fieldType == typeof(Color32)) {
					Color32 color2 = default(Color32);
					__instance.m_Primitive.Color32(ref color2);
					field.SetValue(obj, color2);
					return false;
				}
				if (fieldType == typeof(Vector2)) {
					Vector2 vector = default(Vector2);
					__instance.m_Primitive.Vector(ref vector);
					field.SetValue(obj, vector);
					return false;
				}
				if (fieldType == typeof(Vector3)) {
					Vector3 vector2 = default(Vector3);
					__instance.m_Primitive.Vector(ref vector2);
					field.SetValue(obj, vector2);
					return false;
				}
				if (fieldType == typeof(Vector4)) {
					Vector4 vector3 = default(Vector4);
					__instance.m_Primitive.Vector(ref vector3);
					field.SetValue(obj, vector3);
					return false;
				}
				if (fieldType == typeof(Vector2Int)) {
					Vector2Int vector2Int = default(Vector2Int);
					__instance.m_Primitive.VectorInt(ref vector2Int);
					field.SetValue(obj, vector2Int);
					return false;
				}
				if (fieldType == typeof(Gradient)) {
					Gradient value3 = null;
					__instance.m_Primitive.Gradient(ref value3);
					field.SetValue(obj, value3);
					return false;
				}
				if (fieldType == typeof(AnimationCurve)) {
					AnimationCurve value4 = null;
					__instance.m_Primitive.AnimationCurve(ref value4);
					field.SetValue(obj, value4);
					return false;
				}
				if (fieldType == typeof(ColorBlock)) {
					ColorBlock colorBlock = default(ColorBlock);
					__instance.m_Primitive.ColorBlock(ref colorBlock);
					field.SetValue(obj, colorBlock);
					return false;
				}
				if (fieldType.IsOrSubclassOf<UnityEngine.Object>()) {
					UnityEngine.Object value5 = null;
					__instance.m_Primitive.UnityObject<UnityEngine.Object>(ref value5);
					field.SetValue(obj, value5);
					return false;
				}
				if (fieldType == typeof(int[])) {
					int[] value6 = null;
					__instance.m_Primitive.IntArray(ref value6);
					field.SetValue(obj, value6);
					return false;
				}
				if (fieldType == typeof(uint[])) {
					uint[] value7 = null;
					__instance.m_Primitive.UIntArray(ref value7);
					field.SetValue(obj, value7);
					return false;
				}
				if (fieldType == typeof(float[])) {
					float[] value8 = null;
					__instance.m_Primitive.FloatArray(ref value8);
					field.SetValue(obj, value8);
					return false;
				}
				if (fieldType == typeof(long[])) {
					long[] value9 = null;
					__instance.m_Primitive.LongArray(ref value9);
					field.SetValue(obj, value9);
					return false;
				}
				if (fieldType == typeof(ulong[])) {
					ulong[] value10 = null;
					__instance.m_Primitive.ULongArray(ref value10);
					field.SetValue(obj, value10);
					return false;
				}
				if (fieldType == typeof(string[])) {
					string[] value11 = null;
					__instance.m_Primitive.StringArray(ref value11);
					field.SetValue(obj, value11);
					return false;
				}
				if (fieldType == typeof(bool[])) {
					bool[] value12 = null;
					__instance.m_Primitive.BoolArray(ref value12);
					field.SetValue(obj, value12);
					return false;
				}
				if (fieldType.IsArray && fieldType.GetElementType().IsEnum) {
					int[] array = null;
					__instance.m_Primitive.IntArray(ref array);
					Array array2 = Array.CreateInstance(fieldType.GetElementType(), array.Length);
					for (int i = 0; i < array.Length; i++) {
						array2.SetValue(Enum.ToObject(fieldType.GetElementType(), array[i]), i);
					}
					field.SetValue(obj, array2);
					return false;
				}
				if (fieldType == typeof(Color[])) {
					Color[] value13 = null;
					__instance.m_Primitive.ColorArray(ref value13);
					field.SetValue(obj, value13);
					return false;
				}
				if (fieldType == typeof(Color32[])) {
					Color32[] value14 = null;
					__instance.m_Primitive.Color32Array(ref value14);
					field.SetValue(obj, value14);
					return false;
				}
				if (fieldType == typeof(Vector2[])) {
					Vector2[] value15 = null;
					__instance.m_Primitive.VectorArray(ref value15);
					field.SetValue(obj, value15);
					return false;
				}
				if (fieldType == typeof(Vector3[])) {
					Vector3[] value16 = null;
					__instance.m_Primitive.VectorArray(ref value16);
					field.SetValue(obj, value16);
					return false;
				}
				if (fieldType == typeof(Vector4[])) {
					Vector4[] value17 = null;
					__instance.m_Primitive.VectorArray(ref value17);
					field.SetValue(obj, value17);
					return false;
				}
				if (fieldType.IsArray && fieldType.GetElementType().IsOrSubclassOf<UnityEngine.Object>()) {
					int num7 = 0;
					__instance.m_Primitive.Int(ref num7);
					Array array3 = Array.CreateInstance(fieldType.GetElementType(), num7);
					for (int j = 0; j < num7; j++) {
						UnityEngine.Object value18 = null;
						__instance.m_Primitive.UnityObject<UnityEngine.Object>(ref value18);
						array3.SetValue(value18, j);
					}
					field.SetValue(obj, array3);
					return false;
				}
				if (fieldType.IsListOf<string>()) {
					List<string> value19 = null;
					__instance.m_Primitive.StringList(ref value19);
					field.SetValue(obj, value19);
					return false;
				}
				if (fieldType.IsListOf<int>()) {
					List<int> value20 = null;
					__instance.m_Primitive.IntList(ref value20);
					field.SetValue(obj, value20);
					return false;
				}
				if (fieldType.IsList() && fieldType.GetGenericArguments()[0].IsEnum) {
					int[] array4 = null;
					__instance.m_Primitive.IntArray(ref array4);
					IList list = (IList)Activator.CreateInstance(fieldType);
					foreach (int value21 in array4) {
						list.Add(Enum.ToObject(fieldType.GetGenericArguments()[0], value21));
					}
					field.SetValue(obj, list);
					return false;
				}
				if (fieldType.IsListOf<Color32>()) {
					List<Color32> value22 = null;
					__instance.m_Primitive.Color32List(ref value22);
					field.SetValue(obj, value22);
					return false;
				}
				if (fieldType.IsList() && fieldType.GenericTypeArguments[0].IsOrSubclassOf<UnityEngine.Object>()) {
					int num8 = 0;
					__instance.m_Primitive.Int(ref num8);
					IList list2 = (IList)Activator.CreateInstance(fieldType);
					for (int l = 0; l < num8; l++) {
						UnityEngine.Object value23 = null;
						__instance.m_Primitive.UnityObject<UnityEngine.Object>(ref value23);
						list2.Add(value23);
					}
					field.SetValue(obj, list2);
					return false;
				}
				if (fieldType.IsArray) {
					int num9 = 0;
					__instance.m_Primitive.Int(ref num9);
					Array array6 = Array.CreateInstance(fieldType.GetElementType(), num9);
					for (int m = 0; m < num9; m++) {
						object value24 = null;
						__instance.GenericObject(ref value24, fieldType.GetElementType());
						array6.SetValue(value24, m);
					}
					field.SetValue(obj, array6);
					return false;
				}
				if (fieldType.IsList()) {
					int num10 = 0;
					__instance.m_Primitive.Int(ref num10);
					IList list3 = (IList)Activator.CreateInstance(fieldType);
					for (int n = 0; n < num10; n++) {
						object value25 = null;
						__instance.GenericObject(ref value25, fieldType.GenericTypeArguments[0]);
						list3.Add(value25);
					}
					field.SetValue(obj, list3);
					return false;
				}
				if (GuidClassBinder.IsIdentifiedType(fieldType) || fieldType.HasAttribute<SerializableAttribute>()) {
					object value26 = null;
					__instance.GenericObject(ref value26, fieldType);
					field.SetValue(obj, value26);
					return false;
				}
				PFLog.Default.Error(string.Concat(new string[]
				{
				"Cannot serialize field ",
				field.Name,
				" of ",
				field.DeclaringType.Name,
				": unrecognized field type"
				}), Array.Empty<object>());
				return false;
			}
		}
#endif
	}
}
