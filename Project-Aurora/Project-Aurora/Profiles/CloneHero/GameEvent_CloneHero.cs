using Aurora.Profiles.CloneHero.GSI;
using Aurora.Utils;

/*
 * Clone Hero support added by @Joey4305
 */

namespace Aurora.Profiles.CloneHero
{
    public class GameEvent_CloneHero : MemoryReadingLightEvent<CloneHeroPointers, GameState_CloneHero>
    {
        public GameEvent_CloneHero() : base("CloneHero.json", "Clone Hero", "UnityPlayer.dll", true) { }

        public override void UpdateGameState(GameState_CloneHero gameState, MemoryReader reader) {
            int streak = gameState.Player.NoteStreak = reader.ReadInt(pointers.NoteStreak);

            #region NoteStreak Extras

            // Breaks up the note streak into the 1x, 2x, 3x, 4x zones for easy lighting options
            if (streak >= 0 && streak <= 10) {
                gameState.Player.NoteStreak1x = streak;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 10) {
                    gameState.Player.NoteStreak2x = 10;
                }
            } else if (streak > 10 && streak <= 20) {
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = streak - 10;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 20) {
                    gameState.Player.NoteStreak3x = 10;
                }
            } else if (streak > 20 && streak <= 30) {
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = streak - 20;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 30) {
                    gameState.Player.NoteStreak4x = 10;
                }
            } else if (streak > 30 && streak <= 40) {
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = streak - 30;
            }
            #endregion

            gameState.Player.IsStarPowerActive = reader.ReadInt(pointers.IsStarPowerActive) == 1 ? true : false;

            gameState.Player.StarPowerPercent = reader.ReadFloat(pointers.StarPowerPercent) * 100;

            gameState.Player.IsAtMenu = reader.ReadInt(pointers.IsAtMenu) == 1 ? true : false;

            gameState.Player.NotesTotal = reader.ReadInt(pointers.NotesTotal);

            #region FC Indicator

            // Resets at the beginning of a new song
            if (gameState.Player.NoteStreak == 0 && gameState.Player.NotesTotal == 0) {
                gameState.Player.IsFC = true;
            }

            // Doing this check is necessary to prevent a weird bug
            if (gameState.Player.IsFC) {
                if (gameState.Player.NoteStreak < gameState.Player.NotesTotal) {
                    gameState.Player.IsFC = false;
                } else {
                    gameState.Player.IsFC = true;
                }
            }

            #endregion

            gameState.Player.TotalNotesHit = reader.ReadInt(pointers.TotalNotesHit);

            gameState.Player.Score = reader.ReadInt(pointers.Score);

            gameState.Player.IsGreenPressed = reader.ReadInt(pointers.IsGreenPressed) > 0 ? true : false;
            gameState.Player.IsRedPressed = reader.ReadInt(pointers.IsRedPressed) > 0 ? true : false;
            gameState.Player.IsYellowPressed = reader.ReadInt(pointers.IsYellowPressed) > 0 ? true : false;
            gameState.Player.IsBluePressed = reader.ReadInt(pointers.IsBluePressed) > 0 ? true : false;
            gameState.Player.IsOrangePressed = reader.ReadInt(pointers.IsOrangePressed) > 0 ? true : false;
            System.Console.WriteLine(reader.ReadInt(pointers.IsGreenPressed));
        }
    }
}
