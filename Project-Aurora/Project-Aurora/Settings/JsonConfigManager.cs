using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

using Aurora.Settings.Bindables;
using Aurora.Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Settings
{
    public class JsonConfigManager : ConfigManager<string>
    {
        private string path;

        private string filename = "main.json";
        protected virtual string Filename => Path.Combine(path, filename);

        public JsonConfigManager(string path, IDictionary<string, object> defaultOverrides) : base(defaultOverrides)
        {
            if (path.Contains(".json"))
            {
                filename = new FileInfo(path).Name;
                this.path = path.Replace(filename, "");
            }
            else this.path = path;
            InitialiseDefaults();
            Load();
        }

        protected override void PerformLoad()
        {
            if (string.IsNullOrEmpty(Filename) || !File.Exists(Filename)) return;

            using (var reader = new StreamReader(Filename))
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                foreach (var jToken in jObject)
                {
                    if (ConfigStore.TryGetValue(jToken.Key, out IBindable b))
                    {
                        switch (b)
                        {
                            case BindableBindableDictionary d:
                                if (jToken.Value.HasValues)
                                {
                                    foreach (var jTokenInner in jToken.Value.ToObject<JObject>())
                                    {
                                        if (d.TryGetValue(jTokenInner.Key, out IBindable bd))
                                        {
                                            bd.Parse(jTokenInner.Value);
                                        }
                                        else if (AddMissingEntries)
                                            Set(jToken.Key, jToken.Value);
                                    }
                                }
                                break;
                            default:
                                b.Parse(jToken.Value);
                                break;
                        }
                    }
                    else if (AddMissingEntries)
                        Set(jToken.Key, jToken.Value);
                }
            }
        }

        protected override bool PerformSave()
        {
            if (string.IsNullOrEmpty(Filename)) return false;

            try
            {
                using (var w = new StreamWriter(Filename))
                {
                    w.Write(JsonConvert.SerializeObject(ConfigStore, Formatting.Indented));
                }
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error performing save on {Filename}: {e.ToString()}");
                return false;
            }

            return true;
        }
    }
}