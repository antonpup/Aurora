namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class WeaponNode : Node
    {
        public readonly WeaponType Type;
        public readonly string ID;
        public readonly int Max;
        public readonly int Current_Clip;
        public readonly int Current_Left;
        public readonly int Max_Clip;
        public readonly bool IsSelected;

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
