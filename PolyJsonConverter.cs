using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParadoxNotion;

namespace PolyConverter
{
    // Handles Poly Bridge 2 objects that don't have a default constructor

    public class PolyJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => Types.Contains(objectType);

        public static readonly IReadOnlyList<Type> Types = new Type[] {
            typeof(BridgeJointProxy)
        };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            object obj = objectType.CreateObjectUninitialized();
            foreach (JProperty property in json.Properties())
            {
                var field = objectType.GetField(property.Name);
                if (field == null) continue;

                if (VectorJsonConverter.Types.Contains(field.FieldType)) // Special case for vectors
                {
                    object v = field.FieldType.CreateObjectUninitialized();
                    foreach (JProperty p in ((JObject)property.Value).Properties())
                    {
                        field.FieldType.GetField(p.Name)?.SetValue(v, p.Value.ToObject<float>());
                    }
                    field.SetValue(obj, v);
                }
                else
                {
                    field.SetValue(obj, property.Value.ToObject(field.FieldType));
                }
            }
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
