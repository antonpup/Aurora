using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class WeaponsNode : Node
    {
        private List<WeaponNode> _Weapons = new List<WeaponNode>();

        public int Count { get { return _Weapons.Count; } }
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
        /// Gets the weapon with index &lt;index&gt;
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
