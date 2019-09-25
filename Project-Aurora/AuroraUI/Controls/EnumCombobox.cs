using System;
using System.Collections.Generic;
using System.Linq;

namespace AuroraUI.Controls {

    public class EnumCombobox<TEnum> : Combobox<TEnum> where TEnum : Enum {

        public EnumCombobox() {
            base.Items = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            DisplayMember = e => e.ToString();
        }

        public new IEnumerable<TEnum> Items {
            get => base.Items;
            set => throw new NotImplementedException("This property does not support setting.");
        }
    }
}
