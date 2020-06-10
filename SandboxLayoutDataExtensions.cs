using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PolyConverter
{
    public static class SandboxLayoutDataExtensions
    {
        // Same as normal serialization, but fixes the vehicle checkpoint GUID serialization
        // which made Unity throw an error
        public static byte[] SerializeBinaryCustom(this object data)
        {
            if (data.GetType() != Program.SandboxLayoutData) throw new ArgumentException(null, "data");

            var bytes = new List<byte>();
            var args = new[] { bytes };
            var binding = BindingFlags.NonPublic | BindingFlags.Instance;
            var layout = Program.SandboxLayoutData;
            var vehicle = Program.VehicleProxy;
            var serializer = Program.ByteSerializer;

            layout.GetMethod("SerializePreBridgeBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeBridgeBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeZedAxisVehiclesBinary", binding).Invoke(data, binding, null, args, null);

            int vehicleCount = ((IEnumerable<object>)layout.GetField("m_Vehicles").GetValue(data)).Count();
            bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeInt").Invoke(null, new object[] { vehicleCount }));
            foreach (var veh in (IEnumerable<object>)layout.GetField("m_Vehicles").GetValue(data))
            {
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeString").Invoke(null, new[] { vehicle.GetField("m_DisplayName").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeVector2").Invoke(null, new[] { vehicle.GetField("m_Pos").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeQuaternion").Invoke(null, new[] { vehicle.GetField("m_Rot").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeString").Invoke(null, new[] { vehicle.GetField("m_PrefabName").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_TargetSpeed").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_Mass").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_BrakingForceMultiplier").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeInt").Invoke(null, new object[] { (int)vehicle.GetField("m_StrengthMethod").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_Acceleration").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_MaxSlope").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_DesiredAcceleration").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_ShocksMultiplier").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_RotationDegrees").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeFloat").Invoke(null, new[] { vehicle.GetField("m_TimeDelaySeconds").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeBool").Invoke(null, new[] { vehicle.GetField("m_IdleOnDownhill").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeBool").Invoke(null, new[] { vehicle.GetField("m_Flipped").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeBool").Invoke(null, new[] { vehicle.GetField("m_OrderedCheckpoints").GetValue(veh) }));
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeString").Invoke(null, new[] { vehicle.GetField("m_Guid").GetValue(veh) }));

                var checkpoints = (IEnumerable<object>)vehicle.GetField("m_CheckpointGuids").GetValue(veh);
                bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeInt").Invoke(null, new object[] { checkpoints.Count() }));
                foreach (var checkpoint in checkpoints)
                {
                    bytes.AddRange((IEnumerable<byte>)serializer.GetMethod("SerializeString").Invoke(null, new[] { checkpoint }));

                }

                bytes.AddRange(bytes);
            }

            layout.GetMethod("SerializeVehicleStopTriggersBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeEventTimelinesBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeCheckpointsBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeTerrainStretchesBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializePlatformsBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeRampsBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeVehicleRestartPhasesBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeFlyingObjectsBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeRocksBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializeWaterBlocksBinary", binding).Invoke(data, binding, null, args, null);
            bytes.AddRange((IEnumerable<byte>)Program.BudgetProxy.GetMethod("SerializeBinary").Invoke(layout.GetField("m_Budget").GetValue(data), null));
            bytes.AddRange((IEnumerable<byte>)Program.SandboxSettingsProxy.GetMethod("SerializeBinary").Invoke(layout.GetField("m_Settings").GetValue(data), null));
            layout.GetMethod("SerializeCustomShapesBinary", binding).Invoke(data, binding, null, args, null);
            bytes.AddRange((IEnumerable<byte>)Program.WorkshopProxy.GetMethod("SerializeBinary").Invoke(layout.GetField("m_Workshop").GetValue(data), null));
            layout.GetMethod("SerializeSupportPillarsBinary", binding).Invoke(data, binding, null, args, null);
            layout.GetMethod("SerializePillarsBinary", binding).Invoke(data, binding, null, args, null);

            return bytes.ToArray();
        }
    }
}
