using System.ComponentModel;

namespace Aurora.Settings
{
    /// <summary>
    /// Enum list for the percent effect type
    /// </summary>
    public enum PercentEffectType
    {
        /// <summary>
        /// All at once
        /// </summary>
        [Description("All at once")]
        AllAtOnce = 0,

        /// <summary>
        /// Progressive
        /// </summary>
        [Description("Progressive")]
        Progressive = 1,

        /// <summary>
        /// Progressive (Gradual)
        /// </summary>
        [Description("Progressive (Gradual)")]
        Progressive_Gradual = 2,

        [Description("Only highest active key (foreground color)")]
        Highest_Key = 3,

        [Description("Only highest active key (blended color)")]
        Highest_Key_Blend = 4
    }

    public enum IdleEffects
    {
        [Description("None")]
        None = 0,
        [Description("Dim")]
        Dim = 1,
        [Description("Color Breathing")]
        ColorBreathing = 2,
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,
        [Description("Star Fall")]
        StarFall = 5,
        [Description("Rain Fall")]
        RainFall = 6,
        [Description("Blackout")]
        Blackout = 7,
        [Description("Matrix")]
        Matrix = 8,
        [Description("Rain Fall Smooth")]
        RainFallSmooth = 9
    }

    /// <summary>
    /// Enum list for the layer effects
    /// </summary>
    public enum LayerEffects
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        None = 0,

        /// <summary>
        /// Single Color Overlay
        /// </summary>
        [Description("Single Color Overlay")]
        ColorOverlay = 1,

        /// <summary>
        /// Color Breathing
        /// </summary>
        [Description("Color Breathing")]
        ColorBreathing = 2,

        /// <summary>
        /// Rainbow Shift (Horizontal)
        /// </summary>
        [Description("Rainbow Shift (Horizontal)")]
        RainbowShift_Horizontal = 3,

        /// <summary>
        /// Rainbow Shift (Vertical)
        /// </summary>
        [Description("Rainbow Shift (Vertical)")]
        RainbowShift_Vertical = 4,

        /// <summary>
        /// Rainbow Shift (Diagonal)
        /// </summary>
        [Description("Rainbow Shift (Diagonal)")]
        RainbowShift_Diagonal = 5,

        /// <summary>
        /// Rainbow Shift (Other Diagonal)
        /// </summary>
        [Description("Rainbow Shift (Other Diagonal)")]
        RainbowShift_Diagonal_Other = 6,

        /// <summary>
        /// Rainbow Shift (Custom Angle)
        /// </summary>
        [Description("Rainbow Shift (Custom Angle)")]
        RainbowShift_Custom_Angle = 7,

        /// <summary>
        /// Gradient Shift (Custom Angle)
        /// </summary>
        [Description("Gradient Shift (Custom Angle)")]
        GradientShift_Custom_Angle = 8,
    }

    public enum AppExitMode
    {
        [Description("Always Ask")]
        Ask = 0,
        [Description("Always Minimize")]
        Minimize = 1,
        [Description("Always Exit")]
        Exit = 2
    }

    public enum PreferredMouse
    {
        [Description("None")]
        None = 0,

        [Description("Generic Peripheral")]
        Generic_Peripheral = 1,
        [Description("Razer/Corsair Mousepad + Mouse")]
        Generic_Mousepad = 2,

        //Logitech range is 100-199
        [Description("Logitech - G900")]
        Logitech_G900 = 100,
        [Description("Logitech - G502")]
        Logitech_G502 = 101,

        //Corsair range is 200-299
        [Description("Corsair - Sabre")]
        Corsair_Sabre = 200,
        [Description("Corsair - M65")]
        Corsair_M65 = 201,
        [Description("Corsair - Katar")]
        Corsair_Katar = 202,

        //Razer range is 300-399

        //Clevo range is 400-499
        [Description("Clevo - Touchpad")]
        Clevo_Touchpad = 400,

        //Cooler Master range is 500-599

        //Roccat range is 600-699

        //Steelseries range is 700-799
        [Description("SteelSeries - Rival 300")]
        SteelSeries_Rival_300 = 700,
        [Description("SteelSeries - Rival 300 HP OMEN Edition")]
        SteelSeries_Rival_300_HP_OMEN_Edition = 701,
        [Description("SteelSeries - QcK Prism Mousepad + Mouse")]
        SteelSeries_QcK_Prism = 702,
        [Description("SteelSeries - Two-zone QcK Mousepad + Mouse")]
        SteelSeries_QcK_2_Zone = 703,
        //Asus range is 900-999
        [Description("Asus - Pugio")]
        Asus_Pugio = 900
    }



    public enum ApplicationDetectionMode
    {
        [Description("Windows Events (Default)")]
        WindowsEvents = 0,

        [Description("Foreground App Scan")]
        ForegroroundApp = 1
    }
}