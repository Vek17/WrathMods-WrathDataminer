using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    public class LocalizedStringConverter : JsonConverter
    {
        public LocalizedStringConverter() { }
        static LocalizationPack russianPack = null;
        static LocalizationPack RuPack()
        {
            if (russianPack != null) return russianPack;
            var locale = Locale.ruRU;
            string path = Path.Combine(ApplicationPaths.streamingAssetsPath, "Localization/" + locale + ".json");
            using (StreamReader streamReader = new StreamReader(path))
            {
                LocalizationPack localizationPack = JsonConvert.DeserializeObject<LocalizationPack>(streamReader.ReadToEnd());
                localizationPack.Locale = locale;
                russianPack = localizationPack;
            }
            return russianPack;
        }
        public override void WriteJson(JsonWriter w, object o, JsonSerializer szr)
        {
            try
            {
                var ls = (LocalizedString)o;
                for (int i = 0; i < 50 && ls.Shared != null; i++)
                {
                    ls = ls.Shared.String;
                }
                var pack = LocalizationManager.CurrentPack;
                var text = pack.GetText(ls.Key, false);
                if (string.IsNullOrEmpty(text))
                {
                    var ru = RuPack();
                    text = ru.GetText(ls.Key, false);
                }
                w.WriteValue($"LocalizedString:{ls.Key}:{text}");
            } catch(Exception ex)
            {
                w.WriteValue($"LocalizedString:{ex.ToString()}");
            }
        }

        public override object ReadJson(JsonReader reader, Type type, object existing, JsonSerializer serializer)
        {
            string text = (string)reader.Value;
            if (text == null || text == "null")
            {
                return null;
            }
            if(text.StartsWith("LocalizedString") || text.StartsWith("CustomString"))
            {
                var parts = text.Split(':');
                if (parts.Length < 2) return null;
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(parts[1]);
                return localizedString;
            }
            else
            {
                var localizedString = new LocalizedString();
                Traverse.Create(localizedString).Field("m_Key").SetValue(text);
                return localizedString;
            }
        }

        public override bool CanConvert(Type type)
        {
            return typeof(LocalizedString).IsAssignableFrom(type);
        }
    }
}