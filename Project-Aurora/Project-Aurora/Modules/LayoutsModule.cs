using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public partial class LayoutsModule : IAuroraModule
{
    [Async]
    public void Initialize()
    {
        Global.logger.Info("Loading KB Layouts");
        Global.kbLayout = new KeyboardLayoutManager();
        Global.kbLayout.LoadBrandDefault();
        Global.logger.Info("Loaded KB Layouts");
    }
    
    [Async]
    public void Dispose()
    {
        //noop
    }
}