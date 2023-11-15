using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using System.Drawing;
using Aurora.Settings.Overrides.Logic.Boolean;
using Common.Devices;

namespace Aurora.Profiles.Discord;

public class DiscordProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();

        //makes the color red when mic is muted
        var overrideLookupTableBuilder = new OverrideLookupTableBuilder<Color>();
        overrideLookupTableBuilder.AddEntry(Color.Red, new BooleanGSIBoolean("User/SelfMute"));

        OverlayLayers = new System.Collections.ObjectModel.ObservableCollection<Layer>
        {
            new("Mic Status", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.Lime,
                    _Sequence = new KeySequence(new[] { DeviceKeys.PAUSE_BREAK })
                }
            }, new OverrideLogicBuilder()
                .SetDynamicBoolean("_Enabled", new StringComparison
                {
                    Operand1 = new StringGSIString { VariablePath = "Voice/Name" },
                    Operand2 = new StringConstant { Value = "" },
                    Operator = StringComparisonOperator.NotEqual,
                }).SetLookupTable("_PrimaryColor", overrideLookupTableBuilder)),

            new("Mentions", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.Yellow,
                    _Sequence = new KeySequence(new[] { DeviceKeys.PRINT_SCREEN })
                }
            }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/Mentions"))),

            new("Unread Messages", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.LightBlue,
                    _Sequence = new KeySequence(new[] { DeviceKeys.PRINT_SCREEN, DeviceKeys.SCROLL_LOCK, DeviceKeys.PAUSE_BREAK })
                }
            }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/UnreadMessages"))),
        };
    }
}