using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Utils;

namespace Aurora.Profiles.Borderlands2
{
    public class GameEvent_Borderlands2 : MemoryReadingLightEvent<Borderlands2Pointers, GameState_Borderlands2> {

        public GameEvent_Borderlands2() : base("Borderlands2.json", "Borderlands2") { }

        public override void UpdateGameState(GameState_Borderlands2 gameState, MemoryReader reader) {
            gameState.Player.MaximumHealth = reader.ReadFloat(pointers.Health_maximum);
            gameState.Player.CurrentHealth = reader.ReadFloat(pointers.Health_current);
            gameState.Player.MaximumShield = reader.ReadFloat(pointers.Shield_maximum);
            gameState.Player.CurrentShield = reader.ReadFloat(pointers.Shield_current);
        }
    }

    public class Borderlands2Pointers {
        public PointerData Health_maximum;
        public PointerData Health_current;
        public PointerData Shield_maximum;
        public PointerData Shield_current;
    }
}
