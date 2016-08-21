using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// A class representing a default settings layer
    /// </summary>
    public class DefaultLayer
    {
        internal string Name { get; set; }
        internal UserControl Control { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DefaultLayer()
        {
            Name = "New Layer";
            Control = new Control_DefaultLayer();
        }
    }
}
