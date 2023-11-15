namespace Aurora.Settings.Overrides.Logic;

/// <summary>
/// Interaction logic for Control_ConditionGSIBoolean.xaml
/// </summary>
public partial class Control_ConditionGSIBoolean
{
    public Control_ConditionGSIBoolean(BooleanGSIBoolean context) {
        InitializeComponent();
        DataContext = context;
    }
}