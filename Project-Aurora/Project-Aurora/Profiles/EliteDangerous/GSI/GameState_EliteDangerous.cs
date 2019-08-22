using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous.GSI
{
    class GameState_EliteDangerous : GameState<GameState_EliteDangerous>
    {
        private Status status;
        
        public Status Status
        {
            get
            {
                if (status == null)
                    status = new Status();

                return status;
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
