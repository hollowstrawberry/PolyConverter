using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PolyConverter
{
    /// <summary> Necessary to remove redundant properties in Unity vectors, which would
    /// otherwise throw an error or cause an infinite loop.</summary>
    public class VectorJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType) => Types.Contains(objectType);

        public static readonly IReadOnlyList<Type> Types = new[] {
            Program.Vector2, Program.Vector3, Program.Quaternion, Program.Color
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var values = new Dictionary<string, float>();
            foreach (var field in value.GetType().GetFields())
                if (field.Name.Length == 1) // x y z w r g b a
                    values.Add(field.Name, (float)field.GetValue(value));
            JObject json = JObject.FromObject(values);
            json.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            object obj = FormatterServices.GetUninitializedObject(objectType);
            foreach (JProperty property in json.Properties())
                objectType.GetField(property.Name)?.SetValue(obj, property.Value.ToObject<float>());
            return obj;
        }
    }
}
