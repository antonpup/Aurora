namespace Aurora.Settings.Overrides.Logic {

    public interface IOverrideLogic : System.ComponentModel.INotifyPropertyChanged {
        /// <summary>
        /// Evalutes this logic and returns the value of the first lookup which has a truthy condition.
        /// Will return `null` if there are no true conditions.
        /// </summary>
        object Evaluate(Profiles.IGameState gameState);

        /// <summary>
        /// Gets a control for editing this overridge logic system.
        /// </summary>
        System.Windows.Media.Visual Control { get; }
    }
}
