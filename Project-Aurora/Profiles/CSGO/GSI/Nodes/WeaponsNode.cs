using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing information about player's weapons
    /// </summary>
    public class WeaponsNode : Node
    {
        private List<WeaponNode> _Weapons = new List<WeaponNode>();

        /// <summary>
        /// The number of weapons a player has in their inventory
        /// </summary>
        public int Count { get { return _Weapons.Count; } }

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

                return new WeaponNode("");
            }
        }

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
