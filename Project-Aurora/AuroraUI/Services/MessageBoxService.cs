using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuroraUI.Services {

    /// <summary>
    /// Main service that is responsible for opening message boxes.
    /// </summary>
    public class MessageBoxService {

        /// <summary>Event that is fired when a dependant has requested a message box to open.</summary>
        public event Action<MessageBoxParameters> MessageBoxOpen;

        public void Open(string title, string content) => MessageBoxOpen?.Invoke(new MessageBoxParameters(title, content));
        public void Open(string title, RenderFragment content) => MessageBoxOpen?.Invoke(new MessageBoxParameters(title, content));
    }


    /// <summary>
    /// Class which contains the data for the MessageBox, such as the content and the title.
    /// </summary>
    public class MessageBoxParameters {

        public string Title { get; }
        public RenderFragment Content { get; }
        public bool CanClose { get; }

        public MessageBoxParameters(string title, RenderFragment content) {
            Title = title;
            Content = content;
        }

        public MessageBoxParameters(string title, string content) : this(title, b => {
            b.OpenElement(0, "p");
            b.AddContent(1, content);
            b.CloseElement();
        }) { }
    }


    /// <summary>
    /// Extension method to register the <see cref="MessageBoxService"/>.
    /// </summary>
    public static class MessageBoxServiceCollectionExtension {
        public static IServiceCollection AddMessageBox(this IServiceCollection sc) => sc.AddScoped<MessageBoxService>();
    }
}
