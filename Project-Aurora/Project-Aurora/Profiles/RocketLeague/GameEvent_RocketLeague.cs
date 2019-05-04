using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;
using Aurora.Utils;
using System;

namespace Aurora.Profiles.RocketLeague
{
    public class GameEvent_RocketLeague : MemoryReadingLightEvent<RocketLeaguePointers, GameState_RocketLeague>
    {
        public GameEvent_RocketLeague() : base("RocketLeague.json", "RocketLeague") { }

        public override void UpdateGameState(GameState_RocketLeague gameState, MemoryReader reader) {
            PlayerTeam parsed_team = PlayerTeam.Undefined;
            if (Enum.TryParse(reader.ReadInt(pointers.Team.baseAddress, pointers.Team.pointers).ToString(), out parsed_team))
                gameState.Player.Team = parsed_team;
            
            // Goal explosion preperation
            gameState.Match.YourTeam_LastScore = parsed_team == PlayerTeam.Blue ? gameState.Match.BlueTeam_Score : gameState.Match.OrangeTeam_Score;
            gameState.Match.EnemyTeam_LastScore = parsed_team == PlayerTeam.Orange ? gameState.Match.BlueTeam_Score : gameState.Match.OrangeTeam_Score;

            gameState.Match.OrangeTeam_Score = reader.ReadInt(pointers.Orange_score);
            gameState.Match.BlueTeam_Score = reader.ReadInt(pointers.Blue_score);
            gameState.Player.BoostAmount = reader.ReadInt(pointers.Boost_amount);
        }
    }
}
