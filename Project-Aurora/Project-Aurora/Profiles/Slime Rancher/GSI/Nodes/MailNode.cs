using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public class MailNode : Node<MailNode> {

        public bool NewMail;

        internal MailNode(string json) : base(json) {
            NewMail = GetBool("new_mail");

        }
    }
}
