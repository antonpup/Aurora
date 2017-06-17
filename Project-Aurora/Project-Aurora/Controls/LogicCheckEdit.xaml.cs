using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for LogicCheckEdit.xaml
    /// </summary>
    public partial class LogicCheckEdit : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty LayerProperty = DependencyProperty.Register("Layer", typeof(Layer), typeof(LogicCheckEdit));

        public Layer Layer
        {
            get
            {
                return (Layer)GetValue(LayerProperty);
            }
            set
            {
                SetValue(LayerProperty, value);
                this.cmbParameter.ItemsSource = value.AssociatedApplication.ParameterLookup
                    .Where(s => (s.Value.Item1.IsPrimitive || s.Value.Item1 == typeof(string)) && s.Value.Item2 == null).ToDictionary(s => s) //Remove methods and non-primitives for right now
                    .Keys;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty LogicProperty = DependencyProperty.Register("Check", typeof(KeyValuePair<string, Tuple<LogicOperator, object>>), typeof(LogicCheckEdit));

        public KeyValuePair<string, Tuple<LogicOperator, object>> Check
        {
            get
            {
                return (KeyValuePair<string, Tuple<LogicOperator, object>>)GetValue(LogicProperty);
            }
            set
            {
                SetValue(LogicProperty, value);
                this.cmbParameter.SelectedItem = value.Key;
                this.cmbCheck.SelectedItem = Check.Value.Item1;
            }
        }

        public LogicCheckEdit()
        {
            InitializeComponent();
        }

        private void cmbParameter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            string str = e.AddedItems[0] as string;
            string old_str = e.RemovedItems[0] as string;
            Tuple<Type, Type> typ = Layer.AssociatedApplication.ParameterLookup[str];
            Tuple<Type, Type> old_typ = old_str != null ? Layer.AssociatedApplication.ParameterLookup[str] : null;
            if (old_typ == null || old_typ.Item1 != typ.Item1)
            {
                List<LogicOperator> operators = new List<LogicOperator>();
                switch (Type.GetTypeCode(typ.Item1))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        operators = ((LogicOperator[])Enum.GetValues(typeof(LogicOperator))).ToList();
                        break;
                    //case TypeCode.Object:
                    case TypeCode.Boolean:
                    case TypeCode.String:
                        operators.Add(LogicOperator.Equal);
                        operators.Add(LogicOperator.NotEqual);
                        break;
                }
                this.cmbCheck.ItemsSource = operators;

                this.grdValue.Children.Clear();
            }
        }
    }
}
