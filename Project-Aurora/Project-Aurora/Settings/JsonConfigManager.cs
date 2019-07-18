using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

using Newtonsoft.Json;

namespace Aurora.Settings
{
    public class JsonConfigManager<T> : ConfigManager<T>
    {
        private string path;
        protected virtual string Filename => @"main.json";

        public JsonConfigManager(string path, IDictionary<T, object> defaultOverrides) : base(defaultOverrides)
        {
            this.path = path;
            InitialiseDefaults();
            Load();
        }

        protected override void PerformLoad()
        {

        }

        protected override bool PerformSave()
        {
            if (string.IsNullOrEmpty(Filename)) return false;

            try
            {
                using (var w = new StreamWriter(Path.Combine(path, Filename)))
                {
                    w.Write(JsonConvert.SerializeObject(ConfigStore));
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}