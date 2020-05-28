using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {

    public class JournalNode : AutoJsonNode<JournalNode>
    {
        public bool QuestAvailable;
        public bool NewQuestAvailable;
        public int QuestCount;

        internal JournalNode(string json) : base(json) { }
    }
}