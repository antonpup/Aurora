using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous.GSI
{
    public class GameState_EliteDangerous : GameState<GameState_EliteDangerous>
    {
        private Status status;
        private Journal journal;
        private Nodes.Controls controls;
        
        public Journal Journal
        {
            get
            {
                if (journal == null)
                    journal = new Journal();

                return journal;
            }
        }
        
        public Status Status
        {
            get
            {
                if (status == null)
                    status = new Status();

                return status;
            }
        }
        
        public Nodes.Controls Controls
        {
            get
            {
                if (controls == null)
                    controls = new Nodes.Controls();

                return controls;
            }
        }
        /// <summary>
        /// Creates a default GameState_EliteDangerous instance.
        /// </summary>
        public GameState_EliteDangerous() : base()
        {
        }
    }
}
