using System;
using System.Collections.Generic;
using System.Text;

namespace Aurora.Utils
{
    public interface IInitialize : IDisposable
    {
        bool Initialized { get; }

        bool Initialize();
    }
}
