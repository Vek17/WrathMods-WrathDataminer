using System;
using Kingmaker.Blueprints;
using Newtonsoft.Json;

namespace CustomBlueprints
{
    public class BlueprintReferenceConverter : JsonConverter
    {

        public BlueprintReferenceConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            var bp = o as BlueprintReferenceBase;
            if (bp == null)
            {
                w.WriteValue((object)null);
            }
            else
            {
                w.WriteValue(string.Format($"Blueprint:{bp.Guid}:{bp.GetBlueprint()?.name ?? "NULL"}"));
            }
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
        private static readonly Type _tBlueprintReferenceBase = typeof(BlueprintReferenceBase);
        public override bool CanConvert(Type type) => _tBlueprintReferenceBase.IsAssignableFrom(type);
    }
}