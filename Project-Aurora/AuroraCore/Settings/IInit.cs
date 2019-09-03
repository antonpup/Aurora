using System;

namespace Aurora.Settings
{
    public interface IInit : IDisposable
    {
        bool Initialized { get; }

        bool Initialize();
    }
}
