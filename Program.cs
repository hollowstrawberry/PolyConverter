using System;
using System.Reflection;

namespace PolyConverter
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Booting up PolyConverter");
            PolyBridge2Assembly = Assembly.LoadFrom("D:\\Games\\SteamLibrary\\steamapps\\common\\Poly Bridge 2\\Poly Bridge 2_Data\\Managed\\Assembly-CSharp.dll");
            UnityAssembly = Assembly.LoadFrom("D:\\Games\\SteamLibrary\\steamapps\\common\\Poly Bridge 2\\Poly Bridge 2_Data\\Managed\\UnityEngine.CoreModule.dll");

            new PolyConverter().Run();
        }

        public static Assembly PolyBridge2Assembly { get; private set; }
        public static Assembly UnityAssembly { get; private set; }

        public static Type SandboxLayoutData => PolyBridge2Assembly.GetType("SandboxLayoutData");
        public static Type ByteSerializer => PolyBridge2Assembly.GetType("ByteSerializer");
        public static Type VehicleProxy => PolyBridge2Assembly.GetType("VehicleProxy");
        public static Type BudgetProxy => PolyBridge2Assembly.GetType("BudgetProxy");
        public static Type SandboxSettingsProxy => PolyBridge2Assembly.GetType("SandboxSettingsProxy");
        public static Type WorkshopProxy => PolyBridge2Assembly.GetType("WorkshopProxy");

        public static Type Vector2 => UnityAssembly.GetType("UnityEngine.Vector2");
        public static Type Vector3 => UnityAssembly.GetType("UnityEngine.Vector3");
        public static Type Quaternion => UnityAssembly.GetType("UnityEngine.Quaternion");
        public static Type Color => UnityAssembly.GetType("UnityEngine.Color");
    }
}
