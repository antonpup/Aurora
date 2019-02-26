using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.GSI.Nodes;
using Aurora.Utils;

namespace Aurora.Profiles.ResidentEvil2 {

    public class GameEvent_ResidentEvil2 : MemoryReadingLightEvent<ResidentEvil2Pointers, GameState_ResidentEvil2> {

        public GameEvent_ResidentEvil2() : base("ResidentEvil2.json", "re2") { }

        public override void UpdateGameState(GameState_ResidentEvil2 gameState, MemoryReader reader) {
            int maxHealth = gameState.Player.MaximumHealth = reader.ReadInt(pointers.HealthMaximum);
            int currentHealth = gameState.Player.CurrentHealth = reader.ReadInt(pointers.HealthCurrent);
            gameState.Player.Poison = reader.ReadInt(pointers.PlayerPoisoned) == 1;
            gameState.Player.Rank = reader.ReadInt(pointers.RankCurrent);


            if (maxHealth == 0) { // not in-game
                gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.OffGame;
                gameState.Player.Rank = 0;
            } else if (currentHealth == 1200) gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.Fine;
            else if (currentHealth > 800) gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.LiteFine;
            else if (currentHealth > 360) gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.Caution;
            else if (currentHealth > 0) gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.Danger;
            else gameState.Player.Status = Player_ResidentEvil2.PlayerStatus.Dead;

            if (gameState.Player.Status == Player_ResidentEvil2.PlayerStatus.Dead) {
                gameState.Player.Poison = false;
                gameState.Player.Rank = 0;
            }
        }
    }
}
