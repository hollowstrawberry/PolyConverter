using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParadoxNotion;
using UnityEngine;

namespace PolyConverter
{
    // Necessary to remove redundant properties in Unity vectors, which would case infinite loops

    public class VectorJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType) => Types.Contains(objectType);

        public static readonly IReadOnlyList<Type> Types = new Type[] {
            typeof(Vector2), typeof(Vector3), typeof(Quaternion),
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var values = new Dictionary<string, float>();
            foreach (var field in value.GetType().GetFields())
                if (field.Name.Length == 1) // coordinates have length 1
                    values.Add(field.Name, (float)field.GetValue(value));
            JObject json = JObject.FromObject(values);
            json.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            object obj = objectType.CreateObjectUninitialized();
            foreach (JProperty property in json.Properties())
            {
                objectType.GetField(property.Name)?.SetValue(obj, property.Value.ToObject<float>());
            }
            return obj;
        }
    }
}
