using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Controls;

/// <summary>
/// Interaction logic for Control_ProfileControlPresenter.xaml
/// </summary>
public partial class Control_ProfileControlPresenter
{
    private bool isSettingNewLayer;

    protected ApplicationProfile _Profile;

    public ApplicationProfile Profile { get => _Profile;
        set { _Profile = value; SetProfile(value); } }

    public Control_ProfileControlPresenter()
    {
        InitializeComponent();
    }

    public Control_ProfileControlPresenter(ApplicationProfile profile) : this()
    {
        Profile = profile;
        grd_LayerControl.IsHitTestVisible = true;
        grd_LayerControl.Effect = null;
    }

    private void SetProfile(ApplicationProfile profile)
    {
        isSettingNewLayer = true;

        DataContext = profile;
        keybindEditor.Stop();
        keybindEditor.ContextKeybind = profile.TriggerKeybind;

        isSettingNewLayer = false;
    }

    private void ResetProfile()
    {
        if (IsLoaded && !isSettingNewLayer && DataContext != null)
        {
            if (MessageBox.Show($"Are you sure you want to reset the \"{Profile.ProfileName}\" profile?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Profile?.Reset();
                //SetProfile(this.Profile);
            }
        }
    }

    private void btnReset_Click(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !isSettingNewLayer && sender is Button)
            ResetProfile();
    }

    private void Control_Keybind_KeybindUpdated(object? sender, Keybind newKeybind)
    {
        if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ApplicationProfile)
        {
            (DataContext as ApplicationProfile).TriggerKeybind = newKeybind;
        }
    }

    private void buttonResetKeybind_Click(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ApplicationProfile)
        {
            Keybind newkb = new Keybind();
            keybindEditor.Stop();
            (DataContext as ApplicationProfile).TriggerKeybind = newkb;
            keybindEditor.ContextKeybind = newkb;
        }
    }

    private void grd_LayerControl_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsLoaded)
            keybindEditor.Stop();
    }
}