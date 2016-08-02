using Aurora.Profiles.Aurora_Wrapper;

namespace Aurora.Profiles.TheDivision
{
    public class GameEvent_TheDivision : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TheDivision()
        {
            profilename = "The Division";
        }

        public override bool IsEnabled()
        {
            return (Global.Configuration.ApplicationProfiles[profilename].Settings as TheDivisionSettings).isEnabled;
        }
    }
}
