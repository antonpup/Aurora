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
    /// <summary>
    /// A class representing weapon information
    /// </summary>
    public class WeaponNode : Node
    {
        /// <summary>
        /// Weapon's name
        /// </summary>
        public string Name;

        /// <summary>
        /// Weapon's skin name
        /// </summary>
        public string Paintkit;

        /// <summary>
        /// Weapon type
        /// </summary>
        public WeaponType Type;

        /// <summary>
        /// Curren amount of ammo in the clip
        /// </summary>
        public int AmmoClip;

        /// <summary>
        /// The maximum amount of ammo in the clip
        /// </summary>
        public int AmmoClipMax;

        /// <summary>
        /// The amount of ammo in reserve
        /// </summary>
        public int AmmoReserve;

        /// <summary>
        /// Weapon's state
        /// </summary>
        public WeaponState State;

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

    /// <summary>
    /// Enum list for all types of weapons
    /// </summary>
    public enum WeaponType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// Rifle
        /// </summary>
        Rifle,

        /// <summary>
        /// Sniper rifles
        /// </summary>
        [Description("Sniper Rifle")]
        SniperRifle,

        /// <summary>
        /// Submachine gun
        /// </summary>
        [Description("Submachine Gun")]
        SubmachineGun,

        /// <summary>
        /// Shotgun
        /// </summary>
        Shotgun,

        /// <summary>
        /// Machine gun
        /// </summary>
        [Description("Machine Gun")]
        MachineGun,

        /// <summary>
        /// Pistol
        /// </summary>
        Pistol,

        /// <summary>
        /// Knife
        /// </summary>
        Knife,

        /// <summary>
        /// Grenade
        /// </summary>
        Grenade,

        /// <summary>
        /// C4
        /// </summary>
        C4,

        /// <summary>
        /// Stackable Item
        /// </summary>
        [Description("Stackable Item")]
        StackableItem
    }

    /// <summary>
    /// Enum list for all weapon states
    /// </summary>
    public enum WeaponState
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// Active (in hand)
        /// </summary>
        Active,

        /// <summary>
        /// Holstered
        /// </summary>
        Holstered,

        /// <summary>
        /// Reloading
        /// </summary>
        Reloading
    }
}
