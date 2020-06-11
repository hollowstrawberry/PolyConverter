using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;

namespace PolyConverter
{
    public class Polygons
    {
        public void Stuff()
        {
            var tetrahedron = new[]
            {
                new Vector3(1,1,1), new Vector3(1,-1,-1), new Vector3(-1,1,-1), new Vector3(-1,-1,1)
            };

            var polygons = new Vector3[][]
            {
                new[] { tetrahedron[0], tetrahedron[1], tetrahedron[2] },
                new[] { tetrahedron[1], tetrahedron[2], tetrahedron[3] },
                new[] { tetrahedron[0], tetrahedron[1], tetrahedron[3] },
                new[] { tetrahedron[0], tetrahedron[2], tetrahedron[3] },
            };

            var rots = new List<Quaternion>();
            var centers = new List<Vector3>();
            var shifts = new List<Vector3>();
            int p = 0;
            foreach (var polygon in polygons)
            {
                var mid = new Vector3((polygon[0].X + polygon[1].X) / 2, (polygon[0].Y + polygon[1].Y) / 2, (polygon[0].Z + polygon[1].Z) / 2);
                var center = new Vector3((polygon[2].X + mid.X) / 2, (polygon[2].Y + mid.Y) / 2, (polygon[2].Z + mid.Z) / 2);

                // Align to XY plane
                var normal = Plane.CreateFromVertices(polygon[0], polygon[1], polygon[2]).Normal;
                var angle = (float)Math.Acos(Vector3.Dot(normal, Vector3.UnitZ));
                var axis = Vector3.Normalize(Vector3.Cross(normal, Vector3.UnitZ));
                var rotation = Quaternion.CreateFromAxisAngle(axis, angle);
                var rotationInv = Quaternion.Inverse(rotation);

                for (int i = 0; i < polygon.Length; i++)
                {
                    var q = rotation * new Quaternion(polygon[i], 0f) * rotationInv;
                    polygon[i] = new Vector3(q.X, q.Y, q.Z);
                }

                rots.Add(rotationInv);

                var newMid = new Vector3((polygon[0].X + polygon[1].X) / 2, (polygon[0].Y + polygon[1].Y) / 2, (polygon[0].Z + polygon[1].Z) / 2);
                var newCenter = new Vector3((polygon[2].X + newMid.X) / 2, (polygon[2].Y + newMid.Y) / 2, (polygon[2].Z + newMid.Z) / 2);

                var polyBridgeShift = normal * 0f;
                shifts.Add(polyBridgeShift);

                for (int i = 0; i < polygon.Length; i++)
                {
                    polygon[i] = (polygon[i] - newCenter + center) + polyBridgeShift;
                }
                centers.Add(center + polyBridgeShift);

                p++;
            }

            for (int i=0; i<polygons.Length; i++)
            {
                var pos = new Vector3(6f, 10.25f, 0f) + centers[i];
                Console.WriteLine($@"{{ ""m_Pos"": {{ ""x"": {pos.X}, ""y"": {pos.Y}, ""z"": {pos.Z} }}, ""m_Rot"": {{ ""x"": {rots[i].X}, ""y"": {rots[i].Y}, ""z"": {rots[i].Z}, ""w"": {rots[i].W} }}, ""m_Scale"": {{ ""x"": 1.0, ""y"": 1.0, ""z"": 0.01 }}, ""m_Dynamic"": false, ""m_CollidesWithRoad"": false, ""m_CollidesWithNodes"": false, ""m_Flipped"": false, ""m_RotationDegrees"": 0.0, ""m_Mass"": 0.0, ""m_Bounciness"": 0.0, ""m_PinMotorStrength"": 0.0, ""m_PinTargetVelocity"": 0.0, ""m_Color"": {{ ""r"": {1f*i/3}, ""g"": {1f*i/3}, ""b"": {1f*i/3}, ""a"": 1.0 }}, ""m_PointsLocalSpace"": [ {{ ""x"": {polygons[i][0].X}, ""y"": {polygons[i][0].Y} }}, {{ ""x"": {polygons[i][1].X}, ""y"": {polygons[i][1].Y} }}, {{ ""x"": {polygons[i][2].X}, ""y"": {polygons[i][2].Y} }} ], ""m_StaticPins"": [ {{ ""x"": {pos.X - (shifts[i].X*0.5f)}, ""y"": {pos.Y - (shifts[i].Y*0.5f)}, ""z"": {pos.Z - (shifts[i].Z*0.5f)} }}, {{ ""x"": {pos.X - (shifts[i].X*0.5f)}, ""y"": {pos.Y - (shifts[i].Y*0.5f)}, ""z"": {pos.Z - (shifts[i].Z*0.5f)} }} ], ""m_DynamicAnchorGuids"": [], ""m_UndoGuid"": null }},");
            }

            return;
        }
    }
}
