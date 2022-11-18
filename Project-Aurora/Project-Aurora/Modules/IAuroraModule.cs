using System;
using System.Threading.Tasks;

namespace Aurora.Modules;

public interface IAuroraModule : IDisposable
{
    public Task InitializeAsync();
    public Task DisposeAsync();
}