using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
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

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_LayerLogicEditor.xaml
    /// </summary>
    public partial class Window_LayerLogicEditor : Window
    {
        public Window_LayerLogicEditor(Layers.Layer layer)
        {
            InitializeComponent();
            this.DataContext = layer;
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //this.stckLogic.Children.Clear();
            LogicItem logic = e.NewValue as LogicItem;
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ((Layers.Layer)this.DataContext).Logics.Add(new LogicItem());
        }

        private void btnAddCheck_Click(object sender, RoutedEventArgs e)
        {
            //((LogicItem)this.grdLogicEdit.DataContext).ReferenceComparisons.Add(new Tuple<string, Tuple<LogicOperator, object>>(null, new Tuple<LogicOperator, object>(null, null)));
        }
    }
}
