using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class WeaponNode : Node
    {
        public readonly string Name;
        public readonly string Paintkit;
        public readonly WeaponType Type;
        public readonly int AmmoClip;
        public readonly int AmmoClipMax;
        public readonly int AmmoReserve;
        public readonly WeaponState State;

        internal WeaponNode(string JSON)
            : base(JSON)
        {
            Name = GetString("name");
            Paintkit = GetString("paintkit");
            Type = GetEnum<WeaponType>("type");
            AmmoClip = GetInt("ammo_clip");
            AmmoClipMax = GetInt("ammo_clip_max");
            AmmoReserve = GetInt("ammo_reserve");
            State = GetEnum<WeaponState>("state");
        }
    }

    public enum WeaponType
    {
        Undefined,
        Rifle,
        [Description("Sniper Rifle")]
        SniperRifle,
        [Description("Submachine Gun")]
        SubmachineGun,
        Shotgun,
        [Description("Machine Gun")]
        MachineGun,
        Pistol,
        Knife,
        Grenade,
        C4,
        [Description("Stackable Item")]
        StackableItem
    }

    public enum WeaponState
    {
        Undefined,
        Active,
        Holstered,
        Reloading
    }
}
