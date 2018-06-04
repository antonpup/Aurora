using Aurora.Profiles.ETS2.GSI.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.ETS2.GSI {

    public class GameState_ETS2 : GameState<GameState_ETS2> {

        internal Box<ETS2MemoryStruct> _memdat;
        private GameNode _Game;
        private TruckNode _Truck;
        private TrailerNode _Trailer;
        private JobNode _Job;
        private NavigationNode _Navigation;

        /// <summary>
        /// Information about the game and the telemetry server.
        /// </summary>
        public GameNode Game {
            get {
                if (_Game == null)
                    _Game = new GameNode(_memdat);
                return _Game;
            }
        }

        /// <summary>
        /// Information about the truck the player is driving.
        /// </summary>
        public TruckNode Truck {
            get {
                if (_Truck == null)
                    _Truck = new TruckNode(_memdat);
                return _Truck;
            }
        }

        /// <summary>
        /// Information about the trailer attached to the truck the player is driving.
        /// </summary>
        public TrailerNode Trailer {
            get {
                if (_Trailer == null)
                    _Trailer = new TrailerNode(_memdat);
                return _Trailer;
            }
        }

        /// <summary>
        /// Information about the job the player is contracted on.
        /// </summary>
        public JobNode Job {
            get {
                if (_Job == null)
                    _Job = new JobNode(_memdat);
                return _Job;
            }
        }

        /// <summary>
        /// Information about the current route navigator route.
        /// </summary>
        public NavigationNode Navigation {
            get {
                if (_Navigation == null)
                    _Navigation = new NavigationNode(_memdat);
                return _Navigation;
            }
        }

        /// <summary>
        /// Creates a default GameState_ETS2 instance.
        /// </summary>
        public GameState_ETS2() : base() { }

        /// <summary>
        /// Creates a GameState_ETS2 instance based on the passed JSON data.
        /// </summary>
        /// <param name="json_data">The JSON data to parse.</param>
        public GameState_ETS2(string json_data) : base(json_data) { }

        /// <summary>
        /// Creates a GameState_ETS2 instance based on data from another GateState instance.
        /// </summary>
        /// <param name="other">The GameState to copy.</param>
        public GameState_ETS2(IGameState other) : base(other) { }

        /// <summary>
        /// Creates a GameState_ETS2 instance based on data that has been read from the MemoryMappedFile
        /// into a ETS2MemoryStruct.
        /// </summary>
        /// <param name="memdat">The struct the MemoryMappedFile data has been copied into.</param>
        internal GameState_ETS2(ETS2MemoryStruct memdat) : base() {
            _memdat = new Box<ETS2MemoryStruct> { value = memdat };
        }
    }

    /// <summary>
    /// Class to allow the structure to be passed and stored as a reference instead of value
    /// </summary>
    public class Box<T> {
        public T value;
    }
}
