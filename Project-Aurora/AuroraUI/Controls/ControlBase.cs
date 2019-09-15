using Microsoft.AspNetCore.Components;

namespace AuroraUI.Controls {

    public class ControlBase : ComponentBase {

        [Parameter] public string Class { get; set; } = "";
        [Parameter] public string Style { get; set; } = "";
    }
}
