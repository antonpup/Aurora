using System;
using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Aurora.Utils;
using System.Drawing.Drawing2D;
using Aurora.Settings;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Aurora.Profiles.Dishonored.GSI;
using Aurora.Profiles.Dishonored.GSI.Nodes;

namespace Aurora.Profiles.Dishonored
{
    public class GameEvent_Dishonored : MemoryReadingLightEvent<DishonoredPointers, GameState_Dishonored>
    {
        public GameEvent_Dishonored() : base("Dishonored.json", "Dishonored") { }

        public override void UpdateGameState(GameState_Dishonored gameState, MemoryReader reader)
        {
            gameState.Player.MaximumHealth = reader.ReadInt(pointers.MaximumHealth);
            gameState.Player.CurrentHealth = reader.ReadInt(pointers.CurrentHealth);
            gameState.Player.MaximumMana = reader.ReadInt(pointers.MaximumMana);
            gameState.Player.CurrentMana = reader.ReadInt(pointers.CurrentMana);
            gameState.Player.ManaPots = reader.ReadInt(pointers.ManaPots);
            gameState.Player.HealthPots = reader.ReadInt(pointers.HealthPots);
        }
    }

    public class DishonoredPointers
    {
        public PointerData ManaPots;
        public PointerData HealthPots;
        public PointerData CurrentHealth;
        public PointerData MaximumHealth;
        public PointerData CurrentMana;
        public PointerData MaximumMana;
    }
}