using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class PropertyChangedExEventArgs : EventArgs
    {
        public string PropertyName { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public PropertyChangedExEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public delegate void PropertyChangedExEventHandler(object sender, PropertyChangedExEventArgs e);


    public interface INotifyPropertyChangedEx
    {
        event PropertyChangedExEventHandler PropertyChanged;
    }

    public abstract class NotifyPropertyChangedEx : INotifyPropertyChangedEx
    {
        public event PropertyChangedExEventHandler PropertyChanged;

        protected void InvokePropertyChanged(object oldValue, object newValue, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExEventArgs(propertyName, oldValue, newValue));
        }
    }
}
