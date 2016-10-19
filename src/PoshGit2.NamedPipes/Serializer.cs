using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoshGit2.Settings;
using System;

namespace PoshGit2
{
    internal static class Serializer
    {
        public static JsonSerializer Instance = JsonSerializer.Create(new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new GitSettingsConverter(), new StringEnumConverter() }
        });

        private sealed class GitSettingsConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(IGitPromptSettings);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize<ReadWriteGitPromptSettings>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }
    }
}
