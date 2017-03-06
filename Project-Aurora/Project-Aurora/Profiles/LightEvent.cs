using Aurora.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles
{
    public enum LightEventType
    {
        Overlay,
        Normal,
        Underlay
        
    }

    public interface ILightEvent : IInitialize
    {
        void UpdateLights(EffectsEngine.EffectFrame frame);

        void SetGameState(IGameState newGameState);

        void ResetGameState();

        bool IsEnabled { get; }

        LightEventConfig Config { get; }

        ImageSource Icon { get; }

        void Delete();

    }

    /// <summary>
    /// Class responsible for applying EffectLayers to an EffectFrame based on GameState information.
    /// </summary>
    public class LightEvent : ILightEvent
    {
        public ProfileManager Profile { get; set; }
        public LightEventConfig Config { get; protected set; }
        private ImageSource icon;
        public ImageSource Icon
        {
            get
            {
                return GetIcon();
            }
        }

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
            //TODO: Maybe add a local variable to use for IsEnabled when Profile is absent?
            get { return this.Profile?.Settings?.IsEnabled ?? true; }
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

        public virtual ImageSource GetIcon()
        {
            return icon ?? (icon = new BitmapImage(new Uri("/Aurora;component/" + Config.IconURI, UriKind.Relative)));
        }

        public virtual void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
