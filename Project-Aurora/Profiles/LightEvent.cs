namespace Aurora.Profiles
{
    public class LightEvent
    {
        internal string profilename;
        internal GameState _game_state;

        public virtual void UpdateLights(EffectsEngine.EffectFrame frame)
        {

        }

        public virtual void UpdateLights(EffectsEngine.EffectFrame frame, GameState new_game_state)
        {

        }

        public virtual bool IsEnabled()
        {
            return false;
        }
    }
}
