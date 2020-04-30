﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomBlueprints
{
    public class BlueprintAssetIdConverter : JsonConverter
    {

        public BlueprintAssetIdConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var bp = (BlueprintScriptableObject)o;
            w.WriteValue(string.Format($"Blueprint:{bp.AssetGuid}:{bp.name}"));
        }
        public override object ReadJson(
          JsonReader reader,
          Type objectType,
          object existingValue,
          JsonSerializer serializer
        )
        {
            string text = (string)reader.Value;
            if (text == null || text == "null")
            {
                return null;
            }
            if (text.StartsWith("Blueprint"))
            {
                var parts = text.Split(':');
                BlueprintScriptableObject blueprintScriptableObject = JsonBlueprints.AssetProvider.GetBlueprint(objectType, parts[1]);
                return blueprintScriptableObject;
            }
            if (text.StartsWith("File"))
            {
                var parts = text.Split(':');
                var path = Path.Combine(Main.ModPath, "data", parts[1]);
                var blueprintName = Path.GetFileNameWithoutExtension(path);
                if (JsonBlueprints.Blueprints.ContainsKey(blueprintName))
                {
                    return JsonBlueprints.Blueprints[blueprintName];
                }
                Main.DebugLog($"Reading blueprint from file: {text}");
                var result = JsonBlueprints.Load(path, objectType);
                return result;
            }
            throw new JsonSerializationException(string.Format("Invalid blueprint format {0}", text));
        }
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);
        public override bool CanConvert(Type type) => _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}