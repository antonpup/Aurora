using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_StringComparison.xaml
    /// </summary>
    public partial class Control_StringComparison : UserControl {

        public StringComparison Logic { get; set; }
        public Profiles.Application Application { get; set; }

        public Control_StringComparison(StringComparison context, Profiles.Application application) {
            InitializeComponent();
            
            operatorSelection.ItemsSource = Enum.GetValues(typeof(StringComparisonOperator))
                .Cast<StringComparisonOperator>()
                .ToDictionary(
                    op => typeof(StringComparisonOperator).GetMember(op.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? op.ToString(),
                    op => op
                );

            Logic = context;
            Application = application;
            DataContext = this;
        }

        public void SetApplication(Profiles.Application application) {
            Application = application;
        }
    }
}
