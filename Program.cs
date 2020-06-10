using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PolyConverter
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("[#] Booting up PolyConverter");

            string path = null;

            if (File.Exists(ManualGamePath))
            {
                path = File.ReadAllText(ManualGamePath).Trim() + "\\Poly Bridge 2_Data\\Managed";

                Console.WriteLine($"[#] Grabbed Poly Bridge 2 install location from {ManualGamePath}");
            }
            else
            {
                Exception error = null;

                try { path = GetPolyBridge2SteamPath(); }
                catch (Exception e) { error = e; }

                if (error != null || path == null)
                {
                    Console.WriteLine($"[Fatal Error] Failed to locate Poly Bridge 2 installation folder on Steam.");
                    Console.WriteLine($"You can manually set the location by creating a file called gamepath.txt" +
                        "and writing the location of your game folder in that file, then restarting this program.");
                    if (error != null) Console.WriteLine($"\nError message: {error.Message}");
                    Console.WriteLine("\nThe program will now close.");
                    Environment.Exit(1);
                }

                Console.WriteLine($"[#] Automatically detected Poly Bridge 2 installation");
            }

            try
            {
                PolyBridge2Assembly = Assembly.LoadFrom($"{path}\\Assembly-CSharp.dll");
                UnityAssembly = Assembly.LoadFrom($"{path}\\UnityEngine.CoreModule.dll");

                object testObject = FormatterServices.GetUninitializedObject(VehicleProxy);
                VehicleProxy.GetField("m_Pos").SetValue(testObject, Activator.CreateInstance(Vector2));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Fatal Error] Failed to load Poly Bridge 2 libraries at \"{path}\":\n{e}");
                Console.WriteLine($"\nThe program will now close.");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Console.WriteLine();

            try
            {
                while (true)
                {
                    Console.WriteLine("\n");

                    new PolyConverter().Run();

                    Console.WriteLine("\n[#] Press Enter to run the program again.");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Fatal Error] {e}\n\nThe program will now close.");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        public static string GetPolyBridge2SteamPath()
        {
            var paths = new List<string>(10);
            paths.Add((string)Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Valve\\Steam").GetValue("InstallPath"));

            string config = File.ReadAllText($"{paths[0]}\\config\\config.vdf");
            foreach (Match match in Regex.Matches(config, "\"BaseInstallFolder_[0-9]\"\\s+\"([^\"]+)\""))
            {
                paths.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
            }

            foreach (string path in paths)
            {
                string assembliesPath = $"{path}\\steamapps\\common\\Poly Bridge 2\\Poly Bridge 2_Data\\Managed";
                if (Directory.Exists(assembliesPath)) return assembliesPath;
            }

            return null;
        }


        const string ManualGamePath = "gamepath.txt";

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
