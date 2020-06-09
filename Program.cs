using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PolyConverter
{
    class Program
    {
        public static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new JsonConverter[] { new VectorJsonConverter(), new PolyJsonConverter() },
        };

        const string layoutExtension = ".layout";
        const string jsonExtension = ".layout.json";
        const string backupExtension = "_ORIGINAL.layout";

        static readonly List<char> logChars = new List<char> { 'F', 'E', '+', '@', '*', '.', '>' };

        static readonly Regex layoutExtensionRegex = new Regex(layoutExtension.Replace(".", "\\.") + "$");
        static readonly Regex jsonExtensionRegex = new Regex(jsonExtension.Replace(".", "\\.") + "$");
        static readonly Regex backupExtensionRegex = new Regex(backupExtension.Replace(".", "\\.") + "$");

        static void Main()
        {
            while (true)
            {
                var resultLog = new List<string>();
                int count = 0, backups = 0;

                string[] files = null;
                try { files = Directory.GetFiles("."); }
                catch (IOException e)
                {
                    Console.WriteLine($"[Fatal Error] Couldn't access files: {e.Message}.\nThe program will exit.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }

                Console.WriteLine("[>] Working...");

                foreach (string path in files)
                {
                    if (backupExtensionRegex.IsMatch(path))
                    {
                        backups++;
                        continue;
                    }
                    else if (jsonExtensionRegex.IsMatch(path))
                    {
                        string layoutPath = jsonExtensionRegex.Replace(path, layoutExtension);
                        string backupPath = jsonExtensionRegex.Replace(path, backupExtension);
                        if (File.Exists(layoutPath) && !File.Exists(backupPath))
                        {
                            try { File.Copy(layoutPath, backupPath); }
                            catch (IOException e)
                            {
                                resultLog.Add($"[Error] Failed to create backup file \"{PathTrim(backupPath)}\": {e.Message}. Aborting conversion.");
                                continue;
                            }
                            resultLog.Add($"[@] Made backup \"{PathTrim(backupPath)}\"");
                        }

                        try { resultLog.Add(JsonToLayout(path, layoutPath)); }
                        catch (Exception e)
                        {
                            resultLog.Add($"[Fatal Error] Couldn't convert \"{PathTrim(path)}\". See below for details.\n///{e}\n///");
                            continue;
                        }

                        count++;
                    }
                    else if (layoutExtensionRegex.IsMatch(path))
                    {
                        string newPath = layoutExtensionRegex.Replace(path, jsonExtension);
                        if (File.Exists(newPath)) continue;

                        try { resultLog.Add(LayoutToJson(path, newPath)); }
                        catch (Exception e)
                        {
                            resultLog.Add($"[Fatal Error] Couldn't convert \"{PathTrim(path)}\". See below for details.\n///{e}\n///");
                            continue;
                        }
                        count++;
                    }
                }

                // Order the log messages
                for (int i = 0; i < resultLog.Count; i++)
                    if (!logChars.Contains(resultLog[i][1]))
                        resultLog[i] = $"[{logChars.Last()}] {resultLog[i]}"; // failsafe for my dumb log system
                resultLog = resultLog.OrderBy(x => logChars.IndexOf(x[1])).ToList();

                foreach (string msg in resultLog)
                    Console.WriteLine(msg);

                if (count == 0)
                {
                    if (backups == 0) Console.WriteLine("[>] There are no layout files to convert in this folder.");
                    else Console.WriteLine("[>] The only layouts detected are backups and were ignored.");
                }
                else Console.WriteLine($"[>] Done.");

                Console.WriteLine("\nPress Enter to run the program again, or close the window to exit.\n\n\n");
                Console.ReadLine();
            }
        }

        static string LayoutToJson(string layoutPath, string jsonPath)
        {
            int _ = 0;
            var bytes = File.ReadAllBytes(layoutPath);
            var data = new SandboxLayoutData(bytes, ref _);
            string json = JsonConvert.SerializeObject(data, jsonSerializerSettings);

            // Limit the indentation depth to 4 levels for compactness
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){6,}", " ");
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){4,}(\\}|\\])", " $3");

            try { File.WriteAllText(jsonPath, json); }
            catch (IOException e)
            {
                return $"[Error] Failed to save file \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            return $"[+] Created \"{PathTrim(jsonPath)}\"";
        }

        static string JsonToLayout(string jsonPath, string layoutPath)
        {
            string json = File.ReadAllText(jsonPath);
            SandboxLayoutData data = null;

            try { data = JsonConvert.DeserializeObject<SandboxLayoutData>(json, jsonSerializerSettings); }
            catch (JsonReaderException e)
            {
                return $"[Error] Invalid json content in \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            var bytes = data.SerializeBinaryCustom();

            bool existed = File.Exists(layoutPath);
            if (existed)
            {
                var oldBytes = File.ReadAllBytes(layoutPath);
                if (oldBytes.SequenceEqual(bytes))
                {
                    return $"[.] No changes detected in \"{PathTrim(jsonPath)}\"";
                }
            }
            try { File.WriteAllBytes(layoutPath, bytes); }
            catch (IOException e)
            {
                return $"[Error] Failed to save file \"{PathTrim(layoutPath)}\": {e.Message}";
            }

            if (existed) return $"[*] Applied changes to \"{PathTrim(layoutPath)}\"";
            else return $"[*] Converted json file into \"{PathTrim(layoutPath)}\"";
        }

        static string PathTrim(string path)
        {
            return path.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
        }
    }
}
