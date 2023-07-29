using Aurora.Profiles.Witcher3.GSI;
using Witcher3Gsi;

namespace Aurora.Profiles.Witcher3;

public sealed class GameEventWitcher3 : LightEvent
{
    private Witcher3GameStateListener _gameStateListener;

    //The mod this uses was taken from https://github.com/SpoinkyNL/Artemis/, with Spoinky's permission
    public GameEventWitcher3()
    {
        _gameStateListener = new Witcher3GameStateListener();
        _gameStateListener.GameStateChanged += GameStateListenerOnGameStateChanged;
        _gameStateListener.StartReading();
    }

    private void GameStateListenerOnGameStateChanged(object? sender, Witcher3StateEventArgs e)
    {
        var player = e.GameState.Player;

        if (_game_state is not GameStateWitcher3 gameState)
        {
            return;
        }

        var gameStatePlayer = gameState.Player;

        gameStatePlayer.MaximumHealth = player.MaximumHealth;
        gameStatePlayer.CurrentHealth = player.CurrentHealth;
        gameStatePlayer.Stamina = player.Stamina;
        gameStatePlayer.Toxicity = player.Toxicity;
        gameStatePlayer.ActiveSign = player.ActiveSign;
    }

    public override void ResetGameState()
    {
        _game_state = new GameStateWitcher3();
    }

    public override void Dispose()
    {
        base.Dispose();

        _gameStateListener.GameStateChanged -= GameStateListenerOnGameStateChanged;
        _gameStateListener.StopListening();
        _gameStateListener = null;
    }
}