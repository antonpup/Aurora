using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes
{
    public class ProviderNode : Node<ProviderNode>
    {
        public string Name;
        public int AppID;
        //public ModVerNode ModVer;

        internal ProviderNode(string json) : base(json)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
            //ModVer = new ModVerNode(_ParsedData["modver"]?.ToString() ?? "");
        }

        public class ModVerNode : Node<ModVerNode>
        {
            public int Major;
            public int Minor;
            public int Revision;

            internal ModVerNode(string json) : base(json)
            {
                this.Major = GetInt("major");
                this.Minor = GetInt("minor");
                this.Revision = GetInt("revision");
            }
        }
    }
}
