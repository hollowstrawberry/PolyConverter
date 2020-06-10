using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PolyConverter
{
    public class PolyConverter
    {
        public const string LayoutExtension = ".layout";
        public const string JsonExtension = ".layout.json";
        public const string BackupExtension = ".layout.backup";

        public static readonly Regex LayoutRegex = new Regex(LayoutExtension.Replace(".", "\\.") + "$");
        public static readonly Regex JsonRegex = new Regex(JsonExtension.Replace(".", "\\.") + "$");
        public static readonly Regex BackupRegex = new Regex(BackupExtension.Replace(".", "\\.") + "$");

        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new JsonConverter[] { new VectorJsonConverter(), new ProxyJsonConverter() },
        };

        static readonly List<char> LogChars = new List<char> { 'F', 'E', '+', '@', '*', '>' };


        public void ConvertAll()
        {
            var resultLog = new List<string>();
            int fileCount = 0, backups = 0;

            string[] files = null;
            files = Directory.GetFiles(".");

            Console.WriteLine("[>] Working...");

            foreach (string path in files)
            {
                if (BackupRegex.IsMatch(path))
                {
                    backups++;
                    continue;
                }
                else if (JsonRegex.IsMatch(path))
                {
                    string layoutPath = JsonRegex.Replace(path, LayoutExtension);
                    string backupPath = JsonRegex.Replace(path, BackupExtension);

                    resultLog.Add(JsonToLayout(path, layoutPath, backupPath));

                    fileCount++;
                }
                else if (LayoutRegex.IsMatch(path))
                {
                    string newPath = LayoutRegex.Replace(path, JsonExtension);
                    if (File.Exists(newPath)) continue;

                    resultLog.Add(LayoutToJson(path, newPath));

                    fileCount++;
                }
            }

            resultLog = resultLog
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length >= 2)
                .OrderBy(s => LogChars.Contains(s[1]) ? LogChars.IndexOf(s[1]) : LogChars.Last())
                .ToList();

            foreach (string msg in resultLog)
                Console.WriteLine(msg);

            if (resultLog.Count == 0)
            {
                if (fileCount > 0) Console.WriteLine("[>] All files checked, no changes to apply.");
                else if (backups == 0) Console.WriteLine("[>] There are no layout files to convert in this folder.");
                else Console.WriteLine("[>] The only layouts detected are backups and were ignored.");
            }
            else Console.WriteLine($"[>] Done.");
        }


        public string LayoutToJson(string layoutPath, string jsonPath)
        {
            try { return InnerLayoutToJson(layoutPath, jsonPath); }
            catch (Exception e) { return $"[Fatal Error] Couldn't convert \"{PathTrim(layoutPath)}\":\n {e}///"; }
        }

        private string InnerLayoutToJson(string layoutPath, string jsonPath)
        {
            int _ = 0;
            var bytes = File.ReadAllBytes(layoutPath);
            var dataConstructor = Program.SandboxLayoutData.GetConstructor(new[] { typeof(byte).MakeArrayType(), typeof(int).MakeByRefType() });
            var data = dataConstructor.Invoke(new object[] { bytes, _ });
            string json = JsonConvert.SerializeObject(data, JsonSerializerSettings);

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


        public string JsonToLayout(string jsonPath, string layoutPath, string backupPath)
        {
            try { return InnerJsonToLayout(jsonPath, layoutPath, backupPath); }
            catch (Exception e) { return $"[Fatal Error] Couldn't convert \"{PathTrim(layoutPath)}\":\n {e}///"; }
        }

        private string InnerJsonToLayout(string jsonPath, string layoutPath, string? backupPath)
        {
            string json = File.ReadAllText(jsonPath);
            object data;
            try { data = JsonConvert.DeserializeObject(json, Program.SandboxLayoutData, JsonSerializerSettings); }
            catch (JsonReaderException e)
            {
                return $"[Error] Invalid json content in \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            var bytes = data.SerializeBinaryCustom();

            bool madeBackup = false;
            bool existsBefore = File.Exists(layoutPath);

            if (existsBefore)
            {
                var oldBytes = File.ReadAllBytes(layoutPath);
                if (oldBytes.SequenceEqual(bytes))
                {
                    return $"";
                }

                if (backupPath != null && !File.Exists(backupPath))
                {
                    try { File.Copy(layoutPath, backupPath); }
                    catch (IOException e)
                    {
                        return $"[Error] Failed to create backup file \"{PathTrim(backupPath)}\": {e.Message}. Conversion aborted.";
                    }
                    madeBackup = true;
                }
            }

            try { File.WriteAllBytes(layoutPath, bytes); }
            catch (IOException e)
            {
                return $"[Error] Failed to save file \"{PathTrim(layoutPath)}\": {e.Message}";
            }

            if (existsBefore)
            {
                if (madeBackup) return $"[@] Made backup \"{PathTrim(backupPath)}\"\n[*] Applied changes to \"{PathTrim(layoutPath)}\"";
                return $"[*] Applied changes to \"{PathTrim(layoutPath)}\"";
            }
            else return $"[*] Converted json file into \"{PathTrim(layoutPath)}\"";
        }

        public static string PathTrim(string path)
        {
            return path?.Substring(path.LastIndexOfAny(new[] { '/', '\\' }) + 1);
        }
    }
}
