namespace Aurora.Profiles
{
    /// <summary>
    /// Class responsible for applying EffectLayers to an EffectFrame based on GameState information.
    /// </summary>
    public class LightEvent
    {
        internal string profilename = "";
        internal GameState _game_state;

        /// <summary>
        /// Adds new layers to the passed EffectFrame instance based on GameState information.
        /// </summary>
        /// <param name="frame">EffectFrame instance to which layers will be added</param>
        public virtual void UpdateLights(EffectsEngine.EffectFrame frame)
        {

        }

        /// <summary>
        /// Adds new layers to the passed EffectFrame instance based on GameState information as well as process a new GameState instance.
        /// </summary>
        /// <param name="frame">EffectFrame instance to which layers will be added</param>
        /// <param name="new_game_state">GameState instance which will be processed before adding new layers</param>
        public virtual void UpdateLights(EffectsEngine.EffectFrame frame, GameState new_game_state)
        {

        }

        /// <summary>
        /// Returns whether or not this LightEvent is active
        /// </summary>
        /// <returns>A boolean value representing if this LightEvent is active</returns>
        public virtual bool IsEnabled()
        {
            return false;
        }
    }
}
