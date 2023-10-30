using Aurora.Nodes;
using Witcher3Gsi;

namespace Aurora.Profiles.Witcher3.GSI.Nodes;

/// <summary>
/// Class representing player information
/// </summary>
public class PlayerWitcher3 : Node
{
    public int MaximumHealth = 0;
    public int CurrentHealth = 0;
    public float Stamina = 0.0f;
    public float Toxicity = 0.0f;
    public WitcherSign ActiveSign;
}