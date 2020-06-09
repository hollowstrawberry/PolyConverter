using System;
using System.IO;
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

        static readonly string layoutExtension = ".layout";
        static readonly string jsonExtension = ".layout.json";
        static readonly string backupExtension = "_original.layout";

        static readonly Regex layoutExtensionRegex = new Regex(layoutExtension.Replace(".", "\\.") + "$");
        static readonly Regex jsonExtensionRegex = new Regex(jsonExtension.Replace(".", "\\.") + "$");
        static readonly Regex backupExtensionRegex = new Regex(backupExtension.Replace(".", "\\.") + "$");

        static void Main()
        {
            int count = 0;
            string[] files = Directory.GetFiles(".");
            
            foreach (string path in files)
            {
                if (backupExtensionRegex.IsMatch(path))
                    continue;
                else if (jsonExtensionRegex.IsMatch(path))
                {
                    string layoutPath = jsonExtensionRegex.Replace(path, layoutExtension);
                    string backupPath = jsonExtensionRegex.Replace(path, backupExtension);
                    if (File.Exists(layoutPath) && !File.Exists(backupPath)) {
                        File.Copy(layoutPath, backupPath);
                        Console.WriteLine($"Made backup \"{PathTrim(backupPath)}\"");
                    }
                    JsonToLayout(path, layoutPath);
                    count++;
                }
                else if (layoutExtensionRegex.IsMatch(path))
                {
                    string newPath = layoutExtensionRegex.Replace(path, jsonExtension);
                    if (File.Exists(newPath)) continue;
                    LayoutToJson(path, newPath);
                    count++;
                }
            }

            Console.WriteLine(count > 0 ? $"\nDone." : "There are no layout files to convert in this folder.");
            Console.ReadLine();
        }

        static void LayoutToJson(string oldPath, string newPath)
        {
            int _ = 0;
            var bytes = File.ReadAllBytes(oldPath);
            var data = new SandboxLayoutData(bytes, ref _);
            string json = JsonConvert.SerializeObject(data, jsonSerializerSettings);

            // Limit the indentation depth to 4 levels for compactness
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){6,}", " ");
            json = Regex.Replace(json, "(\r\n|\r|\n)( ){4,}(\\}|\\])", " $3");

            File.WriteAllText(newPath, json);
            Console.WriteLine($"Created \"{PathTrim(newPath)}\"");
        }

        static void JsonToLayout(string oldPath, string newPath)
        {
            string json = File.ReadAllText(oldPath);
            var data = JsonConvert.DeserializeObject<SandboxLayoutData>(json, jsonSerializerSettings);
            var bytes = data.SerializeBinaryCustom();
            File.WriteAllBytes(newPath, bytes);
            Console.WriteLine($"Converted json file back into \"{PathTrim(newPath)}\"");
        }

        static string PathTrim(string path)
        {
            return path.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
        }
    }
}
