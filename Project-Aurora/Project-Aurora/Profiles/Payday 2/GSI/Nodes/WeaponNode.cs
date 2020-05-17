namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class WeaponNode : AutoJsonNode<WeaponNode>
    {
        public WeaponType Type;
        public string ID;
        public int Max;
        public int Current_Clip;
        public int Current_Left;
        public int Max_Clip;
        [AutoJsonPropertyName("is_selected")]
        public bool IsSelected;

        internal WeaponNode(string JSON) : base(JSON) { }
    }

    public enum WeaponType
    {
        Undefined,
        Assault_rifle,
        Pistol,
        Smg,
        LMG,
        Snp,
        Akimbo,
        Shotgun,
        Grenade_launcher,
        Saw,
        Minigun,
        Flamethrower,
        Bow,
        Crossbow
    }
}
