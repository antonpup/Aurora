using System;
using System.Collections.Generic;
using Aurora.Plugins;

namespace Aurora.Applications.Layers
{
    public struct LayerHandlerEntry
    {
        public Type Type { get; set; }

        public string Title { get; set; }

        public string Key { get; set; }

        public LayerHandlerEntry(string key, string title, Type type)
        {
            this.Type = type;
            this.Title = title;
            this.Key = key;
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public interface ILayerHandler{}

    public sealed class LayerFactory : IPluginConsumer
    {
        public List<string> DefaultLayerHandlers { get; private set; } = new List<string>();
        
        public Dictionary<string, LayerHandlerEntry> LayerHandlers { get; private set; } = new Dictionary<string, LayerHandlerEntry>();
        
        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static readonly LayerFactory Instance = new LayerFactory();
        
        private LayerFactory() {}
        
        private bool CheckTypeIsLayer(Type type)
        {
            return type.IsSubclassOf(typeof(ILayerHandler));
        }

        public void RegisterLayerHandlers(List<LayerHandlerEntry> layers, bool @default = true)
        {
            foreach(var layer in layers)
            {
                RegisterLayerHandler(layer, @default);
            }
        }

        public bool RegisterLayerHandler(LayerHandlerEntry entry, bool @default = true)
        {
            if (!CheckTypeIsLayer(entry.Type))
                return false;

            if (LayerHandlers.ContainsKey(entry.Key) || DefaultLayerHandlers.Contains(entry.Key))
                return false;

            LayerHandlers.Add(entry.Key, entry);

            if (@default)
                DefaultLayerHandlers.Add(entry.Key);

            return true;
        }

        public bool RegisterLayerHandler(string key, string title, Type type, bool @default = true)
        {
            return RegisterLayerHandler(new LayerHandlerEntry(key, title, type));
        }

        public Type GetLayerHandlerType(string key)
        {
            return LayerHandlers.ContainsKey(key) ? LayerHandlers[key].Type : null;
        }

        public ILayerHandler GetLayerHandlerInstance(LayerHandlerEntry entry)
        {
            return (ILayerHandler)Activator.CreateInstance(entry.Type);
        }

        public ILayerHandler GetLayerHandlerInstance(string key)
        {
            if (LayerHandlers.ContainsKey(key))
            {
                return GetLayerHandlerInstance(LayerHandlers[key]);
            }


            return null;
        }

        public void Visit(PluginBase plugin)
        {
            plugin.Process(this);
        }
    }
}