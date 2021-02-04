using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CustomBlueprints
{
    public class GameObjectAssetIdConverter : JsonConverter
    {

        public GameObjectAssetIdConverter() { }

        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {

            var go = (UnityEngine.Object)o;
            var j = new JObject {
                {"$type", JsonBlueprints.GetTypeName(o.GetType())},
                {"name", go.name },
                {"InstanceId", go.GetInstanceID()},
            };
            j.WriteTo(w);
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanConvert(Type type)
        {
            return typeof(GameObject).IsAssignableFrom(type) ||
                    typeof(Transform).IsAssignableFrom(type);
        }
    }
}