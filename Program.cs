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
        public static int Main(string[] args)
        {
            new Polygons().Stuff();
            Console.ReadLine();
            return 0;

            Console.WriteLine("[#] Booting up PolyConverter");
            
            bool hasArgs = args != null && args.Length > 0;
            string assemblyPath = null;

            if (File.Exists(ManualGamePath))
            {
                try
                {
                    assemblyPath = $"{File.ReadAllLines(ManualGamePath)[0].Trim()}\\Poly Bridge 2_Data\\Managed";
                }
                catch (Exception)
                {
                    Console.WriteLine($"[Fatal Error] Failed to grab Poly Bridge 2 location from {ManualGamePath}");
                    if (!hasArgs)
                    {
                        Console.WriteLine("\n[#] The program will now close.");
                        Console.ReadLine();
                    }
                    return ExitCodeGamePathError;
                }

                Console.WriteLine($"[#] Grabbed Poly Bridge 2 install location from {ManualGamePath}");
            }
            else
            {
                Exception error = null;
                try
                {
                    assemblyPath = GetPolyBridge2SteamPath();
                }
                catch (Exception e) { error = e; }

                if (error != null || assemblyPath == null)
                {
                    Console.WriteLine($"[Fatal Error] Failed to locate Poly Bridge 2 installation folder on Steam.");
                    Console.WriteLine($" You can manually set the location by creating a file called \"{ManualGamePath}\"" +
                        "and writing the location of your game folder in that file, then restarting this program.");
                    if (error != null) Console.WriteLine($"\n[#] Error message: {error.Message}");
                    if (!hasArgs)
                    {
                        Console.WriteLine("\n[#] The program will now close.");
                        Console.ReadLine();
                    }
                    return ExitCodeGamePathError;
                }

                Console.WriteLine($"[#] Automatically detected Poly Bridge 2 installation");
            }

            try
            {
                PolyBridge2Assembly = Assembly.LoadFrom($"{assemblyPath}\\Assembly-CSharp.dll");
                UnityAssembly = Assembly.LoadFrom($"{assemblyPath}\\UnityEngine.CoreModule.dll");

                object testObject = FormatterServices.GetUninitializedObject(VehicleProxy);
                VehicleProxy.GetField("m_Pos").SetValue(testObject, Activator.CreateInstance(Vector2));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Fatal Error] Failed to load Poly Bridge 2 libraries at \"{assemblyPath}\":\n {e}");
                if (!hasArgs)
                {
                    Console.WriteLine("\n[#] The program will now close.");
                    Console.ReadLine();
                }
                return ExitCodeGamePathError;
            }

            Console.WriteLine();

            if (hasArgs)
            {
                string filePath = string.Join(' ', args).Trim();

                if (PolyConverter.JsonRegex.IsMatch(filePath))
                {
                    string newPath = PolyConverter.JsonRegex.Replace(filePath, PolyConverter.LayoutExtension);
                    string backupPath = PolyConverter.JsonRegex.Replace(filePath, PolyConverter.BackupExtension);

                    string result = new PolyConverter().JsonToLayout(filePath, newPath, backupPath);

                    Console.WriteLine(result);
                    if (result.Contains("Invalid json")) return ExitCodeJsonError;
                    if (result.Contains("Error") && result.Contains("file")) return ExitCodeFileError;
                    if (result.Contains("Error")) return ExitCodeConversionError;
                    return ExitCodeSuccessful;
                }
                else if (PolyConverter.LayoutRegex.IsMatch(filePath))
                {
                    string newPath = PolyConverter.LayoutRegex.Replace(filePath, PolyConverter.JsonExtension);

                    string result = new PolyConverter().LayoutToJson(filePath, newPath);

                    Console.WriteLine(result);
                    if (result.Contains("Error") && result.Contains("file")) return ExitCodeFileError;
                    if (result.Contains("Error")) return ExitCodeConversionError;
                    return ExitCodeSuccessful;
                }
                else
                {
                    Console.WriteLine($"[Error] File extension must be either {PolyConverter.LayoutExtension} or {PolyConverter.JsonExtension}");
                    return ExitCodeFileError;
                }
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("\n");

                    new PolyConverter().ConvertAll();

                    Console.WriteLine("\n[#] Press Enter to run the program again.");
                    Console.ReadLine();
                }
            }
        }

        static string GetPolyBridge2SteamPath()
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


        const int ExitCodeSuccessful = 0;
        const int ExitCodeJsonError = 1;
        const int ExitCodeConversionError = 2;
        const int ExitCodeFileError = 3;
        const int ExitCodeGamePathError = 4;

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
