using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes
{
    public class VacPackNode : AutoJsonNode<VacPackNode>
    {
        public SlotNode Amount => NodeFor<SlotNode>("amount");
        public SlotNode Max => NodeFor<SlotNode>("max");
        public ColorNode Color => NodeFor<ColorNode>("color");
        public SpiralNode Spiral => NodeFor<SpiralNode>("spiral");
        
        [AutoJsonPropertyName("sellected_slot")]
        public int SellectedSlot;
        [AutoJsonPropertyName("useable_slots")]
        public int UseableSlots;
        [AutoJsonPropertyName("emty_Item_slots")]
        public int EmtyItemSlots;
        [AutoJsonPropertyName("in_gadget_mode")]
        public bool InGadgetMode;
        [AutoJsonPropertyName("in_nimble_valley_mode")]
        public bool InNimbleValleyMode;

        internal VacPackNode(string json) : base(json) { }

        public class SlotNode : AutoJsonNode<SlotNode>
        {
            [AutoJsonPropertyName("sellected_slot")]
            public int SellectedSlot;
            public int Slot1;
            public int Slot2;
            public int Slot3;
            public int Slot4;
            public int Slot5;

            internal SlotNode(string json) : base(json) { }
        }

        public class ColorNode : AutoJsonNode<ColorNode>
        {
            public ColorSlotNode SellectedSlot => NodeFor<ColorSlotNode>("sellected_slot");
            public ColorSlotNode Slot1 => NodeFor<ColorSlotNode>("slot1");
            public ColorSlotNode Slot2 => NodeFor<ColorSlotNode>("slot2");
            public ColorSlotNode Slot3 => NodeFor<ColorSlotNode>("slot3");
            public ColorSlotNode Slot4 => NodeFor<ColorSlotNode>("slot4");
            public ColorSlotNode Slot5 => NodeFor<ColorSlotNode>("slot5");

            internal ColorNode(string json) : base(json) { }
        }

        public class ColorSlotNode : AutoJsonNode<ColorSlotNode>
        {
            public float Red;
            public float Green;
            public float Blue;
            public float Alpha;

            internal ColorSlotNode(string json) : base(json) { }
        }

        public class SpiralNode : AutoJsonNode<SpiralNode>
        {
            public float Percentage;
            [AutoJsonPropertyName("warning_threshold")]
            public float WarningThreshold;

            internal SpiralNode(string json) : base(json) { }
        }
    }
}
