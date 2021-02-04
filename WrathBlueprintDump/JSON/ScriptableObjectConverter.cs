using System;
using Kingmaker.Blueprints;
using Kingmaker.SimCloth;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomBlueprints
{
    public class ScriptableObjectConverter : JsonConverter
    {

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        public bool CannotRead;
        public override bool CanRead
        {
            get
            {
                return !CannotRead;
            }
        }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }
        public object ReadResource(JsonReader reader, Type objectType, object existingValue, JsonSerializer szr)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer szr)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (type == typeof(EquipmentEntity)
                || type == typeof(BakedCharacter)
                || type == typeof(SimClothTopology))
            {
                return ReadResource(reader, type, existingValue, szr);
            }
            //TODO: Fix json reading of blueprint fields of type ScriptabeObject (BlueprintCueBase.ParentAsset)
            if (reader.TokenType == JsonToken.String)
            {
                return new BlueprintAssetIdConverter().ReadJson(reader, type, existingValue, szr);
            }
            using (new PushValue<bool>(true, () => CannotRead, (canRead) => CannotRead = canRead))
            {
                return szr.Deserialize(reader);
            }
        }
        public override bool CanConvert(Type type)
        {
            return typeof(ScriptableObject).IsAssignableFrom(type)
              && !typeof(BlueprintScriptableObject).IsAssignableFrom(type);
        }
    }
}