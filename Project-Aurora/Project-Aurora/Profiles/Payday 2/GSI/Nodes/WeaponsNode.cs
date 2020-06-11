using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class WeaponsNode : Node
    {
        private List<WeaponNode> _Weapons = new List<WeaponNode>();
        private WeaponNode _ActiveWeapon = new WeaponNode("");


        public int Count { get { return _Weapons.Count; } }
        public WeaponNode SelectedWeapon
        {
            get
            {
                foreach (WeaponNode weapon in _Weapons)
                {
                    if (weapon.IsSelected)
                        _ActiveWeapon = weapon;
                }

                return _ActiveWeapon;
            }
        }

        internal WeaponsNode(string JSON) : base(JSON)
        {
            foreach (JToken jt in _ParsedData.Children())
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
