using Avalonia;
using Avalonia.Markup.Xaml;

namespace AuroraUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}