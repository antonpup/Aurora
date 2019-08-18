using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes
{
    public class VacPackNode : Node<VacPackNode>
    {
        public AmountNode Amount;
        public MaxNode Max;
        public ColorNode Color;

        public int SellectedSlot;
        public int UseableSlots;
        public int EmtyItemSlots;
        public bool InGadgetMode;
        public bool InNimbleValleyMode;

        internal VacPackNode(string json) : base(json)
        {
            Amount = new AmountNode(_ParsedData["amount"]?.ToString() ?? "");
            Max = new MaxNode(_ParsedData["max"]?.ToString() ?? "");
            Color = new ColorNode(_ParsedData["color"]?.ToString() ?? "");

            SellectedSlot = GetInt("sellected_slot");
            UseableSlots = GetInt("useable_slots");
            EmtyItemSlots = GetInt("emty_Item_slots");
            InGadgetMode = GetBool("in_gadget_mode");
            InNimbleValleyMode = GetBool("in_nimble_valley_mode");
        }

        public class AmountNode : Node<AmountNode>
        {
            public int SellectedSlot;
            public int Slot1;
            public int Slot2;
            public int Slot3;
            public int Slot4;
            public int Slot5;

            internal AmountNode(string json) : base(json)
            {
                SellectedSlot = GetInt("sellected_slot");
                Slot1 = GetInt("slot1");
                Slot2 = GetInt("slot2");
                Slot3 = GetInt("slot3");
                Slot4 = GetInt("slot4");
                Slot5 = GetInt("slot5");
            }
        }

        public class MaxNode : Node<MaxNode>
        {
            public int SellectedSlot;
            public int Slot1;
            public int Slot2;
            public int Slot3;
            public int Slot4;
            public int Slot5;

            internal MaxNode(string json) : base(json)
            {
                SellectedSlot = GetInt("sellected_slot");
                Slot1 = GetInt("slot1");
                Slot2 = GetInt("slot2");
                Slot3 = GetInt("slot3");
                Slot4 = GetInt("slot4");
                Slot5 = GetInt("slot5");
            }
        }

        public class ColorNode : Node<ColorNode>
        {
            public SellectedSlotNode SellectedSlot;
            public ColorSlot1Node Slot1;
            public ColorSlot2Node Slot2;
            public ColorSlot3Node Slot3;
            public ColorSlot4Node Slot4;
            public ColorSlot5Node Slot5;

            internal ColorNode(string json) : base(json)
            {
                this.SellectedSlot = new SellectedSlotNode(_ParsedData["sellected_slot"]?.ToString() ?? "");
                this.Slot1 = new ColorSlot1Node(_ParsedData["slot1"]?.ToString() ?? "");
                this.Slot2 = new ColorSlot2Node(_ParsedData["slot2"]?.ToString() ?? "");
                this.Slot3 = new ColorSlot3Node(_ParsedData["slot3"]?.ToString() ?? "");
                this.Slot4 = new ColorSlot4Node(_ParsedData["slot4"]?.ToString() ?? "");
                this.Slot5 = new ColorSlot5Node(_ParsedData["slot5"]?.ToString() ?? "");
            }
        }
        #region ColorSlotNodes
        public class SellectedSlotNode : Node<SellectedSlotNode>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal SellectedSlotNode(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }

        public class ColorSlot1Node : Node<ColorSlot1Node>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlot1Node(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }

        public class ColorSlot2Node : Node<ColorSlot2Node>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlot2Node(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }

        public class ColorSlot3Node : Node<ColorSlot3Node>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlot3Node(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }

        public class ColorSlot4Node : Node<ColorSlot4Node>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlot4Node(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }

        public class ColorSlot5Node : Node<ColorSlot5Node>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlot5Node(string json) : base(json)
            {
                this.Red = GetFloat("red");
                this.Green = GetFloat("green");
                this.Blue = GetFloat("blue");
                this.Alpha = GetFloat("alpha");
            }
        }
        #endregion
    }
}
