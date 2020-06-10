using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PolyConverter
{
    /// <summary> Handles SandboxLayoutData fields whose type don't have a default constructor.</summary>
    public class ProxyJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type type) => 
            type.Assembly == Program.PolyBridge2Assembly
            && type.Name.EndsWith("Proxy")
            && type.GetConstructors().All(c => c.GetParameters().Length > 0);

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
