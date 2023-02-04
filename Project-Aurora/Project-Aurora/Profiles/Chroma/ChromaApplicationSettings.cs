using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Aurora.Settings;

namespace Aurora.Profiles.Chroma;

public class ChromaApplicationSettings: ApplicationSettings
{
    public ObservableCollection<string> ExcludedPrograms { get; set; } = new();
    
    
    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        if (!ExcludedPrograms.Contains("Aurora.exe"))
        {
            ExcludedPrograms.Add("Aurora.exe");
        }
    }
}