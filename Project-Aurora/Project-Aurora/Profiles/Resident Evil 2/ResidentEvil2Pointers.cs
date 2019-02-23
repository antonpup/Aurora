namespace Aurora.Profiles.ResidentEvil2
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class ResidentEvil2Pointers
    {
        public PointerData HealthMaximum;
        public PointerData HealthCurrent;
        public PointerData PlayerPoisoned;
        public PointerData RankCurrent;
    }
}
