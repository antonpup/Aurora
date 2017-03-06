using System;

namespace Aurora.Settings
{
    public interface IInitialize : IDisposable
    {
        bool Initialize();

        bool Initialized { get; }
    }
}