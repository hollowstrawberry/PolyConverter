using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PolyConverter
{
    public class PolyConverter
    {
        public const string LayoutExtension = ".layout";
        public const string LayoutJsonExtension = ".layout.json";
        public const string LayoutBackupExtension = ".layout.backup";
        public const string SlotExtension = ".slot";
        public const string SlotJsonExtension = ".slot.json";
        public const string SlotBackupExtension = ".slot.backup";

        public const string SlotDiffBefore = "25-00-00-00-53-79-73-74-65-6D-2E-42-79-74-65-5B-5D-2C-20-53-79-73-74-65-6D-2E-50-72-69-76-61-74-65-2E-43-6F-72-65-4C";
        public const string SlotDiffAfter = "17-00-00-00-53-79-73-74-65-6D-2E-42-79-74-65-5B-5D-2C-20-6D-73-63-6F-72-6C";

        public static readonly Regex LayoutRegex = new Regex(LayoutExtension.Replace(".", "\\.") + "$");
        public static readonly Regex LayoutJsonRegex = new Regex(LayoutJsonExtension.Replace(".", "\\.") + "$");
        public static readonly Regex LayoutBackupRegex = new Regex(LayoutBackupExtension.Replace(".", "\\.") + "$");
        public static readonly Regex SlotRegex = new Regex(SlotExtension.Replace(".", "\\.") + "$");
        public static readonly Regex SlotJsonRegex = new Regex(SlotJsonExtension.Replace(".", "\\.") + "$");
        public static readonly Regex SlotBackupRegex = new Regex(SlotBackupExtension.Replace(".", "\\.") + "$");

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
                if (LayoutBackupRegex.IsMatch(path) || SlotBackupRegex.IsMatch(path))
                {
                    backups++;
                    continue;
                }
                else if (LayoutJsonRegex.IsMatch(path))
                {
                    string layoutPath = LayoutJsonRegex.Replace(path, LayoutExtension);
                    string backupPath = LayoutJsonRegex.Replace(path, LayoutBackupExtension);

                    if (File.Exists(layoutPath) && File.GetLastWriteTimeUtc(layoutPath) > File.GetLastWriteTimeUtc(path))
                    {
                        continue;
                    }

                    resultLog.Add(JsonToLayout(path, layoutPath, backupPath));

                    fileCount++;
                }
                else if (LayoutRegex.IsMatch(path))
                {
                    string jsonPath = LayoutRegex.Replace(path, LayoutJsonExtension);
                    if (File.Exists(jsonPath) && File.GetLastWriteTimeUtc(jsonPath) > File.GetLastWriteTimeUtc(path))
                    {
                        continue;
                    }

                    resultLog.Add(LayoutToJson(path, jsonPath));

                    fileCount++;
                }
                else if (SlotJsonRegex.IsMatch(path))
                {
                    string slotPath = SlotJsonRegex.Replace(path, SlotExtension);
                    string backupPath = SlotJsonRegex.Replace(path, SlotBackupExtension);

                    if (File.Exists(slotPath) && File.GetLastWriteTimeUtc(slotPath) > File.GetLastWriteTimeUtc(path))
                    {
                        continue;
                    }

                    resultLog.Add(JsonToSlot(path, slotPath, backupPath));

                    fileCount++;
                }
                else if (SlotRegex.IsMatch(path))
                {
                    string jsonPath = SlotRegex.Replace(path, SlotJsonExtension);
                    if (File.Exists(jsonPath) && File.GetLastWriteTimeUtc(jsonPath) > File.GetLastWriteTimeUtc(path))
                    {
                        continue;
                    }

                    resultLog.Add(SlotToJson(path, jsonPath));

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
            var bytes = File.ReadAllBytes(layoutPath);
            var dataConstructor = Program.SandboxLayoutData.GetConstructor(new[] { typeof(byte).MakeArrayType(), typeof(int).MakeByRefType() });
            var data = dataConstructor.Invoke(new object[] { bytes, 0 });
            string json = JsonConvert.SerializeObject(data, JsonSerializerSettings);

            // Limit the indentation depth to 3 levels for compactness
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){6,}", " ");
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){4,}(\\}|\\])", " $3");

            bool existsBefore = File.Exists(jsonPath);
            if (existsBefore)
            {
                var oldJson = File.ReadAllText(jsonPath);
                if (oldJson == json) return "";
            }

            try { File.WriteAllText(jsonPath, json); }
            catch (IOException e)
            {
                return $"[Error] Failed to save file \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            return  $"[+] {(existsBefore ? "Updated" : "Created")} \"{PathTrim(jsonPath)}\"";
        }


        public string JsonToLayout(string jsonPath, string layoutPath, string backupPath)
        {
            try { return InnerJsonToLayout(jsonPath, layoutPath, backupPath); }
            catch (Exception e) { return $"[Fatal Error] Couldn't convert \"{PathTrim(layoutPath)}\":\n {e}///"; }
        }

        private string InnerJsonToLayout(string jsonPath, string layoutPath, string backupPath)
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
                    return "";
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

            try
            {
                File.WriteAllBytes(layoutPath, bytes);
                File.SetLastWriteTimeUtc(jsonPath, DateTime.UtcNow + TimeSpan.FromTicks(1)); // Mark as newer
            }
            catch (IOException e)
            {
                return $"[Error] Failed to save file: {e.Message}";
            }

            if (existsBefore)
            {
                if (madeBackup) return $"[@] Made backup \"{PathTrim(backupPath)}\"\n[*] Applied changes to \"{PathTrim(layoutPath)}\"";
                return $"[*] Applied changes to \"{PathTrim(layoutPath)}\"";
            }
            else return $"[*] Converted json file into \"{PathTrim(layoutPath)}\"";
        }


        public string SlotToJson(string slotPath, string jsonPath)
        {
            try { return InnerSlotToJson(slotPath, jsonPath); }
            catch (Exception e) { return $"[Fatal Error] Couldn't convert \"{PathTrim(slotPath)}\":\n {e}///"; }
        }

        private string InnerSlotToJson(string slotPath, string jsonPath)
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var bytes = File.ReadAllBytes(slotPath);
            var format = Enum.ToObject(Program.DataFormat, 0);
            var slotDeserializer = Program.SerializationUtility.GetMethod("DeserializeValue", new[] { typeof(byte).MakeArrayType(), Program.DataFormat, Program.DeserializationContext });
            slotDeserializer = slotDeserializer.MakeGenericMethod(Program.BridgeSaveSlotData);
            var slot = slotDeserializer.Invoke(null, BindingFlags.Static, null, new object[] { bytes, format, null }, null);
            var slotJson = JObject.FromObject(slot, serializer);

            var bridgeBytes = slotJson.Value<byte[]>("m_Bridge");
            var bridge = Program.BridgeSaveData.GetConstructor(new Type[] { }).Invoke(new object[] { });
            var bridgeDeserialize = Program.BridgeSaveData.GetMethod("DeserializeBinary");
            bridgeDeserialize.Invoke(bridge, new object[] { bridgeBytes, 0 });
            slotJson["m_Bridge"] = JObject.FromObject(bridge, serializer);

            string json = slotJson.ToString();
            // Limit the indentation depth to 4 levels for compactness
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){8,}", " ");
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){6,}(\\}|\\])", " $3");

            bool existsBefore = File.Exists(jsonPath);
            if (existsBefore)
            {
                var oldJson = File.ReadAllText(jsonPath);
                if (oldJson == json) return "";
            }

            try { File.WriteAllText(jsonPath, json); }
            catch (IOException e)
            {
                return $"[Error] Failed to save file \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            return $"[+] {(existsBefore ? "Updated" : "Created")} \"{PathTrim(jsonPath)}\"";
        }


        public string JsonToSlot(string jsonPath, string slotPath, string backupPath)
        {
            try { return InnerSlotToLayout(jsonPath, slotPath, backupPath); }
            catch (Exception e) { return $"[Fatal Error] Couldn't convert \"{PathTrim(slotPath)}\":\n {e}///"; }
        }

        private string InnerSlotToLayout(string jsonPath, string slotPath, string backupPath)
        {
            string json = File.ReadAllText(jsonPath);
            object slot;
            try
            {
                var slotJson = JObject.Parse(json);
                var bridgeJson = slotJson["m_Bridge"].ToString();
                var bridge = JsonConvert.DeserializeObject(bridgeJson, Program.BridgeSaveData, JsonSerializerSettings);
                var bridgeBytes = Program.BridgeSaveData.GetMethod("SerializeBinary").Invoke(bridge, new object[] { });
                slotJson["m_Bridge"] = JToken.FromObject(bridgeBytes);
                slot = JsonConvert.DeserializeObject(slotJson.ToString(), Program.BridgeSaveSlotData, JsonSerializerSettings);
            }
            catch (JsonReaderException e)
            {
                return $"[Error] Invalid json content in \"{PathTrim(jsonPath)}\": {e.Message}";
            }

            var format = Enum.ToObject(Program.DataFormat, 0);
            var slotSerializer = Program.SerializationUtility.GetMethods()
                .FirstOrDefault(x => x.Name == "SerializeValue" && x.GetParameters().Length == 3 && x.GetParameters()[1].ParameterType == Program.DataFormat);
            slotSerializer = slotSerializer.MakeGenericMethod(Program.BridgeSaveSlotData);
            var bytes = (byte[])slotSerializer.Invoke(null, BindingFlags.Static, null, new object[] { slot, format, null }, null);
            bytes = bytes.Replace(SlotDiffBefore, SlotDiffAfter); // Small difference due to the assembly that performs the serialization

            bool madeBackup = false;
            bool existsBefore = File.Exists(slotPath);

            if (existsBefore)
            {
                var oldBytes = File.ReadAllBytes(slotPath);
                if (oldBytes.SequenceEqual(bytes))
                {
                    return "";
                }

                if (backupPath != null && !File.Exists(backupPath))
                {
                    try { File.Copy(slotPath, backupPath); }
                    catch (IOException e)
                    {
                        return $"[Error] Failed to create backup file \"{PathTrim(backupPath)}\": {e.Message}. Conversion aborted.";
                    }
                    madeBackup = true;
                }
            }

            try
            {
                File.WriteAllBytes(slotPath, bytes);
                File.SetLastWriteTimeUtc(jsonPath, DateTime.UtcNow + TimeSpan.FromTicks(1)); // Mark as newer
            }
            catch (IOException e)
            {
                return $"[Error] Failed to save file: {e.Message}";
            }

            if (existsBefore)
            {
                if (madeBackup) return $"[@] Made backup \"{PathTrim(backupPath)}\"\n[*] Applied changes to \"{PathTrim(slotPath)}\"";
                return $"[*] Applied changes to \"{PathTrim(slotPath)}\"";
            }
            else return $"[*] Converted json file into \"{PathTrim(slotPath)}\"";
        }


        public static string PathTrim(string path)
        {
            return path?.Substring(path.LastIndexOfAny(new[] { '/', '\\' }) + 1);
        }
    }
}
