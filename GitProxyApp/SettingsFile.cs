using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace GitProxyApp
{
    [DataContract]
    public class SettingsFile
    {
        public static string DefaultSettingsFilename = "settings.json";
        public static string ConfigurationDirectory = ".gitproxy";

        [DataMember]
        public string SshHostname { get; set; } = "localhost";
        [DataMember]
        public string SshUsername { get; set; } = "user";
        [DataMember]
        public string SshPassword { get; set; } = "";
        [DataMember]
        public string SshKeyFile { get; set; } = null;
        [DataMember]
        public string GitExecutablePath { get; set; } = "git";
        [DataMember]
        public int SshPort { get; set; } = 22;
        [DataMember]
        public string[] GitExtraArguments { get; set; } = null;
        [DataMember]
        public PathMapping[] DirectoryMappings { get; set; } = new PathMapping[]
        {
            new PathMapping("localdir", "remotedir"),
        };
        [DataMember]
        public bool VerboseMode { get; set; } = true;


        public static SettingsFile LoadFromDisk(string filename)
        {
            SettingsFile output;

            JsonSerializer ser = new JsonSerializer();
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs))
            using (JsonReader json = new JsonTextReader(reader))
                output = ser.Deserialize<SettingsFile>(json);

            return output;
        }

        public void SaveToFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            JsonSerializer ser = new JsonSerializer();
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter reader = new StreamWriter(fs))
            using (JsonWriter json = new JsonTextWriter(reader) { Formatting = Formatting.Indented })
                ser.Serialize(json, this);
        }
    }
}
