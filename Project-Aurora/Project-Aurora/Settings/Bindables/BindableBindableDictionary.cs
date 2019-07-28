using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Bindables
{
    public class BindableBindableDictionary : BindableDictionary<string, IBindable>
    {
        public BindableBindableDictionary(Dictionary<string, IBindable> dictionary = null) : base(dictionary)
        {
        }

        public void Add<TBindable>(string key, Bindable<TBindable> value)
        {
            base.Add(key, value);
            value.ValueChanged += _ => InnerValueChanged();
        }

        public new BindableBindableDictionary GetBoundCopy()
        {
            return (BindableBindableDictionary) base.GetBoundCopy();
        }
    }
}
