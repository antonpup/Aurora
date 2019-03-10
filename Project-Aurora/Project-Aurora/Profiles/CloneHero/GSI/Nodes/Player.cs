namespace Aurora.Profiles.CloneHero.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_CloneHero : Node<Player_CloneHero>
    {
        /// <summary>
        /// Player's boost amount [0.0f, 1.0f]
        /// </summary>
        public int NoteStreak = 0;
        public int NoteStreak1x = 0;
        public int NoteStreak2x = 0;
        public int NoteStreak3x = 0;
        public int NoteStreak4x = 0;

        public bool IsStarPowerActive = false;
        public float StarPowerPercent = 0;
        public bool IsAtMenu = false;
        public int NotesTotal = 0;

        public bool IsFC = true;

        public int SoloPercent;

        public int Score;

        public bool IsGreenPressed;
        public bool IsRedPressed;
        public bool IsYellowPressed;
        public bool IsBluePressed;
        public bool IsOrangePressed;

        internal Player_CloneHero(string json_data) : base(json_data)
        {

        }
    }
}
