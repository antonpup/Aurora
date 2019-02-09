namespace Aurora.Profiles.CloneHero
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class CloneHeroPointers
    {
        public PointerData NoteStreak;
        public PointerData SPActivated;
        public PointerData SPPercent;
        public PointerData IsAtMenu;
        public PointerData NotesTotal;
    }
}