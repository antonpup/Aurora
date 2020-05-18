namespace Aurora.Profiles.Witcher3.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_Witcher3 : Node
    {
        public int MaximumHealth = 0;
        public int CurrentHealth = 0;
        public float Stamina = 0.0f;
        public float Toxicity = 0.0f;
        public WitcherSign ActiveSign;
    }

    public enum WitcherSign
    {
        None,
        Axii,
        Aard,
        Igni,
        Quen,
        Yrden
    }
}