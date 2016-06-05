using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class AuthNode : Node
    {
        public readonly string Token;

        internal AuthNode(string JSON)
            : base(JSON)
        {
            Token = GetString("token");
        }

    }
}
