using System.Collections.Generic;
using System.Reflection;

namespace PolyConverter
{
    public static class SandboxLayoutDataExtensions
    {
        // Same as normal serialization, but fixes the vehicle checkpoint GUID serialization
        // which made Unity throw an error
        public static byte[] SerializeBinaryCustom(this SandboxLayoutData data)
        {
            var bytes = new List<byte>();
            var args = new object[] { bytes };
            var binding = BindingFlags.NonPublic | BindingFlags.Instance;
            var type = typeof(SandboxLayoutData);

            type.GetMethod("SerializePreBridgeBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeBridgeBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeZedAxisVehiclesBinary", binding).Invoke(data, binding, null, args, null);

            bytes.AddRange(ByteSerializer.SerializeInt(data.m_Vehicles.Count));
            foreach (VehicleProxy vehicle in data.m_Vehicles)
            {
                List<byte> vehicleBytes = new List<byte>();
                vehicleBytes.AddRange(ByteSerializer.SerializeString(vehicle.m_DisplayName));
                vehicleBytes.AddRange(ByteSerializer.SerializeVector2(vehicle.m_Pos));
                vehicleBytes.AddRange(ByteSerializer.SerializeQuaternion(vehicle.m_Rot));
                vehicleBytes.AddRange(ByteSerializer.SerializeString(vehicle.m_PrefabName));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_TargetSpeed));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_Mass));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_BrakingForceMultiplier));
                vehicleBytes.AddRange(ByteSerializer.SerializeInt((int)vehicle.m_StrengthMethod));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_Acceleration));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_MaxSlope));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_DesiredAcceleration));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_ShocksMultiplier));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_RotationDegrees));
                vehicleBytes.AddRange(ByteSerializer.SerializeFloat(vehicle.m_TimeDelaySeconds));
                vehicleBytes.AddRange(ByteSerializer.SerializeBool(vehicle.m_IdleOnDownhill));
                vehicleBytes.AddRange(ByteSerializer.SerializeBool(vehicle.m_Flipped));
                vehicleBytes.AddRange(ByteSerializer.SerializeBool(vehicle.m_OrderedCheckpoints));
                vehicleBytes.AddRange(ByteSerializer.SerializeString(vehicle.m_Guid));

                vehicleBytes.AddRange(ByteSerializer.SerializeInt(vehicle.m_CheckpointGuids.Count));
                foreach (var checkpoint in vehicle.m_CheckpointGuids)
                {
                    vehicleBytes.AddRange(ByteSerializer.SerializeString(checkpoint));
                }

                bytes.AddRange(vehicleBytes);
            }

            type.GetMethod("SerializeVehicleStopTriggersBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeEventTimelinesBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeCheckpointsBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeTerrainStretchesBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializePlatformsBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeRampsBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeVehicleRestartPhasesBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeFlyingObjectsBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeRocksBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializeWaterBlocksBinary", binding).Invoke(data, binding, null, args, null);
            bytes.AddRange(data.m_Budget.SerializeBinary());
            bytes.AddRange(data.m_Settings.SerializeBinary());
            type.GetMethod("SerializeCustomShapesBinary", binding).Invoke(data, binding, null, args, null);
            bytes.AddRange(data.m_Workshop.SerializeBinary());
            type.GetMethod("SerializeSupportPillarsBinary", binding).Invoke(data, binding, null, args, null);
            type.GetMethod("SerializePillarsBinary", binding).Invoke(data, binding, null, args, null);

            return bytes.ToArray();
        }
    }
}
