namespace Aurora.Profiles.Borderlands2
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class Borderlands2Pointers
    {
        public PointerData Health_maximum;
        public PointerData Health_current;
        public PointerData Shield_maximum;
        public PointerData Shield_current;
    }
}
