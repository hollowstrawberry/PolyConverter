using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PolyConverter
{
    // Handles Poly Bridge 2 objects that don't have a default constructor

    public class PolyJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            if (!objectType.Name.EndsWith("Proxy")) return false;
            var constructors = objectType.GetConstructors();
            return constructors.Length == 2 && constructors.All(c => c.GetParameters().Length > 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            object obj = FormatterServices.GetUninitializedObject(objectType);
            foreach (JProperty property in json.Properties())
            {
                var field = objectType.GetField(property.Name);
                if (field == null) continue;
                field.SetValue(obj, property.Value.ToObject(field.FieldType, serializer));
            }
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
