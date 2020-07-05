using System;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Profiles.EliteDangerous.Layers;

namespace Aurora.Profiles.EliteDangerous.GSI
{
    public class GameStateCondition
    {
        long flagsSet;
        long flagsNotSet;
        GuiFocus[] guiFocus;

        private Func<GameState_EliteDangerous, bool> callback = null;

        public GameStateCondition(long flagsSet = Flag.UNSPECIFIED, long flagsNotSet = Flag.UNSPECIFIED, GuiFocus[] guiFocus = null,
            Func<GameState_EliteDangerous, bool> callback = null)
        {
            this.flagsSet = flagsSet;
            this.guiFocus = guiFocus;
            this.flagsNotSet = flagsNotSet;
            this.callback = callback;
        }

        public GameStateCondition(Func<GameState_EliteDangerous, bool> callback)
        {
            this.callback = callback;
        }

        public bool IsSatisfied(GameState_EliteDangerous gameState)
        {
            if (callback != null && !callback(gameState))
            {
                return false;
            }

            if (guiFocus != null && Array.IndexOf(guiFocus, gameState.Status.GuiFocus) == Flag.UNSPECIFIED)
            {
                return false;
            }

            if (flagsSet != Flag.UNSPECIFIED && !Flag.IsFlagSet(gameState.Status.Flags, flagsSet))
            {
                return false;
            }

            if (flagsNotSet != Flag.UNSPECIFIED && Flag.AtLeastOneFlagSet(gameState.Status.Flags, flagsNotSet))
            {
                return false;
            }

            return true;
        }
    }

    public class NeedsGameState
    {
        public GameStateCondition NeededGameStateCondition;

        public NeedsGameState()
        {
        }

        public NeedsGameState(GameStateCondition neededGameStateCondition)
        {
            this.NeededGameStateCondition = neededGameStateCondition;
        }

        public bool IsSatisfied(GameState_EliteDangerous gameState)
        {
            if (NeededGameStateCondition != null)
            {
                return NeededGameStateCondition.IsSatisfied(gameState);
            }

            return true;
        }
    }

    public class GameState_EliteDangerous : GameState
    {
        private Status status;
        private Nodes.Journal journal;
        private Nodes.Controls controls;

        public Nodes.Journal Journal
        {
            get
            {
                if (journal == null)
                    journal = new Nodes.Journal();

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