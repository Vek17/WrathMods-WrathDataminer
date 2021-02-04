using System;
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
            throw new NotImplementedException();
        }
        private static readonly Type _tBlueprintScriptableObject = typeof(BlueprintScriptableObject);
        public override bool CanConvert(Type type) => _tBlueprintScriptableObject.IsAssignableFrom(type);
    }
}