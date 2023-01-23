using Aurora.Profiles.Witcher3.GSI.Nodes;

namespace Aurora.Profiles.Witcher3.GSI;

/// <summary>
/// A class representing various information retaining to Game State Integration of Witcher 3
/// </summary>
public class GameStateWitcher3 : GameState
{
    public PlayerWitcher3 Player { get; } = new();

    /// <summary>
    /// Creates a default GameState_Witcher3 instance.
    /// </summary>
    public GameStateWitcher3()
    {
    }

    /// <summary>
    /// Creates a GameState instance based on the passed json data.
    /// </summary>
    /// <param name="json_data">The passed json data</param>
    public GameStateWitcher3(string json_data) : base(json_data)
    {
    }
}