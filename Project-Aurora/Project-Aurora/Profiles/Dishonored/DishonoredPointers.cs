namespace Aurora.Profiles.Dishonored
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class DishonoredPointers
    {
        public PointerData ManaPots;
        public PointerData HealthPots;
        public PointerData CurrentHealth;
        public PointerData MaximumHealth;
        public PointerData CurrentMana;
        public PointerData MaximumMana;
    }
}