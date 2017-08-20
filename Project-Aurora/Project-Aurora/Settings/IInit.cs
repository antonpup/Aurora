using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public interface IInit : IDisposable
    {
        bool Initialized { get; }

        bool Initialize();
    }
}
