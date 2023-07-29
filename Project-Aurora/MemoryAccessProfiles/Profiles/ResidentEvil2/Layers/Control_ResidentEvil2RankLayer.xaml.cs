using System.Windows;
using System.Windows.Controls;
using Application = Aurora.Profiles.Application;

namespace MemoryAccessProfiles.Profiles.ResidentEvil2.Layers;

/// <summary>
/// Interaction logic for Control_ResidentEvil2RankLayer.xaml
/// </summary>
public partial class Control_ResidentEvil2RankLayer : UserControl
{
    private bool settingsset = false;

    public Control_ResidentEvil2RankLayer()
    {
        InitializeComponent();
    }

    public Control_ResidentEvil2RankLayer(ResidentEvil2RankLayerHandler datacontext)
    {
        InitializeComponent();

        this.DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (this.DataContext is ResidentEvil2RankLayerHandler && !settingsset)
        {
            settingsset = true;
        }
    }

    internal void SetProfile(Application profile)
    {
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        this.Loaded -= UserControl_Loaded;
    }
}