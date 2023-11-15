using System.Windows;
using System.Windows.Controls;
using Common.Devices;

namespace Aurora.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_ScriptLayer.xaml
/// </summary>
public partial class Control_ScriptLayer
{
    private Profiles.Application Application { get; set; }

    public Control_ScriptLayer()
    {
        InitializeComponent();
    }

    public Control_ScriptLayer(ScriptLayerHandler layerHandler) : this()
    {
        DataContext = layerHandler;
        SetProfile(layerHandler.ProfileManager);
        UpdateScriptSettings();
    }

    public void SetProfile(Profiles.Application application)
    {
        cboScripts.ItemsSource = application.EffectScripts.Keys;
        cboScripts.IsEnabled = application.EffectScripts.Keys.Count > 0;
        Application = application;
    }

    private void cboScripts_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ScriptLayerHandler handler = (ScriptLayerHandler)DataContext;
        handler.OnScriptChanged();
        UpdateScriptSettings();
    }

    private void UpdateScriptSettings()
    {
        ScriptLayerHandler handler = (ScriptLayerHandler)DataContext;
        ScriptPropertiesEditor.RegisteredVariables = handler.GetScriptPropertyRegistry();
        VariableRegistry varReg = ScriptPropertiesEditor.RegisteredVariables;
        ScriptPropertiesEditor.Visibility = varReg == null || varReg.Count == 0 ? Visibility.Hidden : Visibility.Visible;
        ScriptPropertiesEditor.VarRegistrySource = handler.IsScriptValid ? handler.Properties._ScriptProperties : null;
    }

    private void refreshScriptList_Click(object? sender, RoutedEventArgs e) {
        Application.ForceScriptReload();
        cboScripts.Items.Refresh();
        cboScripts.IsEnabled = Application.EffectScripts.Keys.Count > 0;
    }
}