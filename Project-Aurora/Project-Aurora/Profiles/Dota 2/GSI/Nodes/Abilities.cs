using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing hero abilities
    /// </summary>
    public class Abilities_Dota2 : Node, IEnumerable<Ability>
    {
        private List<Ability> abilities = new List<Ability>();

        /// <summary>
        /// The attributes a hero has to spend on abilities
        /// </summary>
        public Attributes Attributes;

        private string json;

        /// <summary>
        /// The number of abilities
        /// </summary>
        public int Count { get { return abilities.Count; } }

        internal Abilities_Dota2(string json_data) : base(json_data)
        {
            json = json_data;

            List<string> abilities = _ParsedData.Properties().Select(p => p.Name).ToList();
            foreach (string ability_slot in abilities)
            {
                if (ability_slot.Equals("attributes"))
                    Attributes = new Attributes(_ParsedData[ability_slot].ToString());
                else
                    this.abilities.Add(new Ability(_ParsedData[ability_slot].ToString()));
            }
        }

        /// <summary>
        /// Gets the ability at a specified index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public Ability this[int index]
        {
            get
            {
                if (index > abilities.Count - 1)
                    return new Ability("");

                return abilities[index];
            }
        }

        public IEnumerator<Ability> GetEnumerator()
        {
            return abilities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return abilities.GetEnumerator();
        }
    }
}
