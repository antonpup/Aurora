using System;
using Aurora.Settings;
using Aurora.Settings.Layers;

namespace Aurora.Profiles;

public class GameEvent_Generic : LightEvent
{

    public override void SetGameState(IGameState new_game_state)
    {
        if (Application.Config.GameStateType != null && !new_game_state.GetType().Equals(Application.Config.GameStateType))
            return;

        _game_state = new_game_state;
        UpdateLayerGameStates();
    }

    private void UpdateLayerGameStates()
    {
        ApplicationProfile settings = Application.Profile;
        if (settings == null)
            return;

        foreach (Layer lyr in settings.Layers)
            lyr.SetGameState(_game_state);

        foreach (Layer lyr in settings.OverlayLayers)
            lyr.SetGameState(_game_state);
    }

    public override void ResetGameState()
    {
        if (Application?.Config?.GameStateType != null)
            _game_state = (IGameState)Activator.CreateInstance(Application.Config.GameStateType);
        else
            _game_state = null;

        UpdateLayerGameStates();
    }
}