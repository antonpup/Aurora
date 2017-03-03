using Aurora.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aurora.Profiles
{
    public enum LightEventType
    {
        Normal,
        Underlay,
        Overlay
    }

    public interface ILightEvent : IInit
    {
        void UpdateLights(EffectsEngine.EffectFrame frame);

        void SetGameState(IGameState newGameState);

        void ResetGameState();

        bool IsEnabled { get; }

        LightEventConfig Config { get; }

    }

    /// <summary>
    /// Class responsible for applying EffectLayers to an EffectFrame based on GameState information.
    /// </summary>
    public class LightEvent : ILightEvent
    {
        public ProfileManager Profile { get; set; }
        public LightEventConfig Config { get; protected set; }

        internal IGameState _game_state;


        public LightEvent()
        {
            this.ResetGameState();
        }

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
        /// <param name="new_game_state">GameState instance which will be processed before adding new layers</param>
        public virtual void SetGameState(IGameState new_game_state)
        {

        }

        /// <summary>
        /// Returns whether or not this LightEvent is active
        /// </summary>
        /// <returns>A boolean value representing if this LightEvent is active</returns>
        public virtual bool IsEnabled
        {
            get { return this.Profile.Settings.IsEnabled; }
        }

        public bool Initialized { get; protected set; }

        public virtual void ResetGameState()
        {
            _game_state = new GameState();
        }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            Initialized = Init();

            return Initialized;
        }

        protected virtual bool Init()
        {

            return true;
        }

        public virtual void Dispose()
        {

        }
    }
}
