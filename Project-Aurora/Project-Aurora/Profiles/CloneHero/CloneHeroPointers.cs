namespace Aurora.Profiles.CloneHero
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class CloneHeroPointers
    {
        // Current Note Streak
        public PointerData NoteStreak;

        // Star Power Pointers
        public PointerData IsStarPowerActive;
        public PointerData StarPowerPercent;

        // At Menu
        public PointerData IsAtMenu;

        // Total Notes Gone By
        public PointerData NotesTotal;

        // Solo Pointers
        public PointerData IsSoloActive; //This one is a UnityPlayer.dll pointer
        public PointerData SoloPercent;

        // Note Pressed Pointers
        public PointerData IsGreenPressed;
        public PointerData IsRedPressed;
        public PointerData IsYellowPressed;
        public PointerData IsBluePressed;
        public PointerData IsOrangePressed;
    }
}