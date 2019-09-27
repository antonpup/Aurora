using AuroraUI.Controls.InputField;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuroraUI.Controls {

    public class EnumCombobox<TEnum> : Combobox<TEnum> {

        public EnumCombobox() {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException("The generic type argument 'TEnum' must be an enum type.");
            base.Items = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            DisplayMember = e => e.ToString();
        }

        public new IEnumerable<TEnum> Items {
            get => base.Items;
            set => throw new NotImplementedException("This property is readonly.");
        }
    }
}
