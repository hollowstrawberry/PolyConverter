using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PolyConverter
{
    public static class SandboxLayoutDataExtensions
    {
        /// <summary> Same as normal serialization, but fixes the vehicle checkpoint GUID serialization,
        /// which would otherwise made Unity throw an error </summary>
        public static byte[] SerializeBinaryCustom(this object data)
        {
            if (data.GetType() != Program.SandboxLayoutData) throw new ArgumentException(null, "data");

            var bytes = new List<byte>();

            SerializeStep(bytes, data, "SerializePreBridgeBinary");
            SerializeStep(bytes, data, "SerializeBridgeBinary");

            SerializeStep(bytes, data, "SerializeZedAxisVehiclesBinary");

            var vehicles = (IEnumerable<object>)Program.SandboxLayoutData.GetField("m_Vehicles").GetValue(data);
            SerializeValue(bytes, vehicles.Count(), "SerializeInt");
            foreach (var vehicle in (IEnumerable<object>)Program.SandboxLayoutData.GetField("m_Vehicles").GetValue(data))
            {
                SerializeVehicleField(bytes, vehicle, "m_DisplayName",            "SerializeString");
                SerializeVehicleField(bytes, vehicle, "m_Pos",                    "SerializeVector2");
                SerializeVehicleField(bytes, vehicle, "m_Rot",                    "SerializeQuaternion");
                SerializeVehicleField(bytes, vehicle, "m_PrefabName",             "SerializeString");
                SerializeVehicleField(bytes, vehicle, "m_TargetSpeed",            "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_Mass",                   "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_BrakingForceMultiplier", "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_StrengthMethod",         "SerializeInt", castToInt: true);
                SerializeVehicleField(bytes, vehicle, "m_Acceleration",           "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_MaxSlope",               "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_DesiredAcceleration",    "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_ShocksMultiplier",       "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_RotationDegrees",        "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_TimeDelaySeconds",       "SerializeFloat");
                SerializeVehicleField(bytes, vehicle, "m_IdleOnDownhill",         "SerializeBool");
                SerializeVehicleField(bytes, vehicle, "m_Flipped",                "SerializeBool");
                SerializeVehicleField(bytes, vehicle, "m_OrderedCheckpoints",     "SerializeBool");
                SerializeVehicleField(bytes, vehicle, "m_Guid",                   "SerializeString");

                var checkpoints = (IEnumerable<object>)Program.VehicleProxy.GetField("m_CheckpointGuids").GetValue(vehicle);
                SerializeValue(bytes, checkpoints.Count(), "SerializeInt");
                foreach (var checkpoint in checkpoints)
                {
                    SerializeValue(bytes, checkpoint, "SerializeString");
                }
            }

            SerializeStep(bytes, data, "SerializeVehicleStopTriggersBinary");
            SerializeStep(bytes, data, "SerializeEventTimelinesBinary");
            SerializeStep(bytes, data, "SerializeCheckpointsBinary");
            SerializeStep(bytes, data, "SerializeTerrainStretchesBinary");
            SerializeStep(bytes, data, "SerializePlatformsBinary");
            SerializeStep(bytes, data, "SerializeRampsBinary");
            SerializeStep(bytes, data, "SerializeVehicleRestartPhasesBinary");
            SerializeStep(bytes, data, "SerializeFlyingObjectsBinary");
            SerializeStep(bytes, data, "SerializeRocksBinary");
            SerializeStep(bytes, data, "SerializeWaterBlocksBinary");
            SerializeField(bytes, data, "m_Budget", Program.BudgetProxy);
            SerializeField(bytes, data, "m_Settings", Program.SandboxSettingsProxy);
            SerializeStep(bytes, data, "SerializeCustomShapesBinary");
            SerializeField(bytes, data, "m_Workshop", Program.WorkshopProxy);
            SerializeStep(bytes, data, "SerializeSupportPillarsBinary");
            SerializeStep(bytes, data, "SerializePillarsBinary");

            return bytes.ToArray();
        }


        /// <summary> Invokes a serialization method of the SandboxLayoutData object using reflection.</summary>
        private static void SerializeStep(List<byte> bytes, object data, string method)
        {
            var binding = BindingFlags.NonPublic | BindingFlags.Instance;
            var args = new object[] { bytes };
            Program.SandboxLayoutData.GetMethod(method, binding).Invoke(data, binding, null, args, null);
        }

        /// <summary> Serializes a field of the SandboxlayoutData object using reflection.</summary>
        private static void SerializeField(List<byte> bytes, object data, string field, Type type)
        {
            var fieldValue = Program.SandboxLayoutData.GetField(field).GetValue(data);
            bytes.AddRange((IEnumerable<byte>)type.GetMethod("SerializeBinary").Invoke(fieldValue, null));
        }

        /// <summary> Serializes a field of the VehicleProxy using reflection.</summary>
        private static void SerializeVehicleField(List<byte> bytes, object vehicle, string field, string method, bool castToInt = false)
        {
            var args = new object[] { Program.VehicleProxy.GetField(field).GetValue(vehicle) };
            if (castToInt) args[0] = (int)args[0]; // special case
            bytes.AddRange((IEnumerable<byte>)Program.ByteSerializer.GetMethod(method).Invoke(null, args));
        }

        /// <summary> Serializes a value using reflection.</summary>
        private static void SerializeValue(List<byte> bytes, object value, string method)
        {
            bytes.AddRange((IEnumerable<byte>)Program.ByteSerializer.GetMethod(method).Invoke(null, new[] { value }));
        }
    }
}
