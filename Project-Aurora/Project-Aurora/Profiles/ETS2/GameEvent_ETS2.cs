using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using System.Diagnostics;
using Aurora.Utils;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Aurora.Profiles.ETS2.GSI;

namespace Aurora.Profiles.ETS2 {
    public class GameEvent_ETS2 : LightEvent {

        /// <summary>Name of the mapped field that the ETS2 Telemetry Server DLL outputs to.</summary>
        private const string memoryMappedFileName = "Local\\Ets2TelemetryServer";

        /// <summary>Target process name (so it can be changed for ATS).</summary>
        private string processName;

        private MemoryMappedFile _memFile;
        private MemoryMappedViewAccessor _memAccessor;

        public GameEvent_ETS2(string processName) : base() {
            this.processName = processName;
        }

        /// <summary>
        /// The MemoryMappedFile shared between this process and the ETS2 Telemetry Server.
        /// </summary>
        private MemoryMappedFile memFile {
            get {
                if (_memFile == null)
                    try { _memFile = MemoryMappedFile.OpenExisting(memoryMappedFileName, MemoryMappedFileRights.ReadWrite); }
                    catch { return null; }
                return _memFile;
            }
        }

        /// <summary>
        /// An Accessor for the shared MemoryMappedFile between this process and the ETS2 Telemetry Server.
        /// </summary>
        private MemoryMappedViewAccessor memAccessor {
            get {
                if (_memAccessor == null && memFile != null)
                    _memAccessor = memFile.CreateViewAccessor(0, Marshal.SizeOf(typeof(ETS2MemoryStruct)), MemoryMappedFileAccess.Read);
                return _memAccessor;
            }
        }

        public override void ResetGameState() {
            _game_state = new GameState_ETS2(default(ETS2MemoryStruct));
        }

        public override void UpdateLights(EffectFrame frame) {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();
            ETS2Profile settings = (ETS2Profile)this.Application.Profile;

            if (Process.GetProcessesByName(processName).Length > 0 && memAccessor != null) {
                // -- Below code adapted from the ETS2 Telemetry Server by Funbit (https://github.com/Funbit/ets2-telemetry-server) --
                IntPtr memPtr = IntPtr.Zero;

                try {
                    byte[] raw = new byte[Marshal.SizeOf(typeof(ETS2MemoryStruct))];
                    memAccessor.ReadArray(0, raw, 0, raw.Length);

                    memPtr = Marshal.AllocHGlobal(raw.Length);
                    Marshal.Copy(raw, 0, memPtr, raw.Length);
                    _game_state = new GameState_ETS2((ETS2MemoryStruct)Marshal.PtrToStructure(memPtr, typeof(ETS2MemoryStruct)));
                }
                finally {
                    if (memPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(memPtr);
                }
                // -- End ETS2 Telemetry Server code --
            }

            foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));

            this.Application.UpdateEffectScripts(layers);
            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state) { }
    }
}
