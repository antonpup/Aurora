namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class WeaponNode : Node<WeaponNode>
    {
        public WeaponType Type;
        public string ID;
        public int Max;
        public int Current_Clip;
        public int Current_Left;
        public int Max_Clip;
        public bool IsSelected;

        internal WeaponNode(string JSON) : base(JSON)
        {
            Type = GetEnum<WeaponType>("type");
            ID = GetString("id");
            Max = GetInt("max");
            Current_Clip = GetInt("current_clip");
            Current_Left = GetInt("current_left");
            Max_Clip = GetInt("max_clip");
            IsSelected = GetBool("is_selected");
        }
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
