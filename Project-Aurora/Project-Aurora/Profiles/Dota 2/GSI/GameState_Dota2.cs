using Aurora.Profiles.Dota_2.GSI.Nodes;
using System;

namespace Aurora.Profiles.Dota_2.GSI
{
    /// <summary>
    /// A class representing various information retaining to Game State Integration of Dota 2
    /// </summary>
    public class GameState_Dota2 : GameState
    {
        private Auth_Dota2 auth;
        private Provider_Dota2 provider;
        private Map_Dota2 map;
        private Player_Dota2 player;
        private Hero_Dota2 hero;
        private Abilities_Dota2 abilities;
        private Items_Dota2 items;
        private GameState_Dota2 previously;
        private GameState_Dota2 added;

        public GameState_Dota2() : base() { }
        public GameState_Dota2(string json_data) : base(json_data) { }

        /// <summary>
        /// Information about GSI authentication
        /// </summary>
        public Auth_Dota2 Auth
        {
            get
            {
                if (auth == null)
                    auth = new Auth_Dota2(GetNode("auth"));

                return auth;
            }
        }

        /// <summary>
        /// Information about the provider of this GameState
        /// </summary>
        public Provider_Dota2 Provider
        {
            get
            {
                if (provider == null)
                    provider = new Provider_Dota2(GetNode("provider"));

                return provider;
            }
        }

        /// <summary>
        /// Information about the current map
        /// </summary>
        public Map_Dota2 Map
        {
            get
            {
                if (map == null)
                    map = new Map_Dota2(GetNode("map"));

                return map;
            }
        }

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Player_Dota2 Player
        {
            get
            {
                if (player == null)
                    player = new Player_Dota2(GetNode("player"));

                return player;
            }
        }

        /// <summary>
        /// Information about the local player's hero
        /// </summary>
        public Hero_Dota2 Hero
        {
            get
            {
                if (hero == null)
                    hero = new Hero_Dota2(GetNode("hero"));

                return hero;
            }
        }

        /// <summary>
        /// Information about the local player's hero abilities
        /// </summary>
        [Range(0, 5)]
        public Abilities_Dota2 Abilities
        {
            get
            {
                if (abilities == null)
                    abilities = new Abilities_Dota2(GetNode("abilities"));

                return abilities;
            }
        }

        /// <summary>
        /// Information about the local player's hero items
        /// </summary>
        public Items_Dota2 Items
        {
            get
            {
                if (items == null)
                    items = new Items_Dota2(GetNode("items"));

                return items;
            }
        }

        /// <summary>
        /// A previous GameState
        /// </summary>
        public GameState_Dota2 Previously
        {
            get
            {
                if (previously == null)
                    previously = new GameState_Dota2(GetNode("previously"));

                return previously;
            }
        }
    }
}
