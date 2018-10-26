using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing information about player's weapons
    /// </summary>
    public class WeaponsNode : Node<WeaponsNode>
    {
        private List<WeaponNode> _Weapons = new List<WeaponNode>();

        /// <summary>
        /// The number of weapons a player has in their inventory
        /// </summary>
        public int Count { get { return _Weapons.Count; } }

        private WeaponNode _ActiveWeaponDummy = new WeaponNode("");

        /// <summary>
        /// Player's currently active weapon
        /// </summary>
        public WeaponNode ActiveWeapon
        {
            get
            {
                foreach(WeaponNode w in _Weapons)
                {
                    if (w.State == WeaponState.Active || w.State == WeaponState.Reloading)
                        return w;
                }

                return _ActiveWeaponDummy;
            }
        }

        public bool HasPrimary => _Weapons.Exists(w => w.Type == WeaponType.Rifle || w.Type == WeaponType.MachineGun || w.Type == WeaponType.SniperRifle || w.Type == WeaponType.SubmachineGun || w.Type == WeaponType.Shotgun);
        public bool HasRifle => _Weapons.Exists(w => w.Type == WeaponType.Rifle);
        public bool HasMachineGun => _Weapons.Exists(w => w.Type == WeaponType.MachineGun);
        public bool HasShotgun => _Weapons.Exists(w => w.Type == WeaponType.Shotgun);
        public bool HasSniper => _Weapons.Exists(w => w.Type == WeaponType.SniperRifle);
        public bool HasKnife => _Weapons.Exists(w => w.Type == WeaponType.Knife);
        public bool HasSMG => _Weapons.Exists(w => w.Type == WeaponType.SubmachineGun);
        public bool HasPistol => _Weapons.Exists(w => w.Type == WeaponType.Pistol);
        public bool HasC4 => _Weapons.Exists(w => w.Type == WeaponType.C4);
        public bool HasGrenade => _Weapons.Exists(w => w.Type == WeaponType.Grenade);
        public int GrenadeCount => _Weapons.Sum(w => (w.Type == WeaponType.Grenade) ? 1 : 0);

        internal WeaponsNode(string JSON)
            : base(JSON)
        {
            foreach(JToken jt in _ParsedData.Children())
            {
                _Weapons.Add(new WeaponNode(jt.First.ToString()));
            }
        }

        /// <summary>
        /// Gets the weapon at a specific index
        /// </summary>
        /// <param name="index">The index to retrieve a weapon at</param>
        /// <returns>A weapon node at the specified index</returns>
        public WeaponNode this[int index]
        {
            get
            {
                if (index > _Weapons.Count - 1)
                {
                    return new WeaponNode("");
                }

                return _Weapons[index];
            }
        }
    }
}
