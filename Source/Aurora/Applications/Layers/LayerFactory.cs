using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aurora.Applications.Application;
using Aurora.EffectsEngine;
using Aurora.Plugins;

namespace Aurora.Applications.Layers
{
    
    public class TitleAttribute : Attribute
    {
        public string Title { get; }

        public TitleAttribute(string title)
        {
            this.Title = title;
        }
    }

    public class ExclusiveLayerHandlerAttribute : Attribute
    {
        public Type ExclusiveGameState { get; }

        public ExclusiveLayerHandlerAttribute(Type exclusiveGameState)
        {
            this.ExclusiveGameState = exclusiveGameState;
        }
    }
    
    //TODO: Duplicate this for PostProcess, Ignoring the Exclusivity stuff. Need to think about if PostProcess will use Gamestate as they need to be used on devices as well.
    public sealed class LayerFactory : IPluginConsumer
    {
        
        private readonly List<(string, Type)> _layerHandlers = new List<(string, Type)>();

        private readonly Dictionary<Type, List<(string, Type)>> _gameStateLayerHandlers = new Dictionary<Type, List<(string, Type)>>();

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static readonly LayerFactory Instance = new LayerFactory();
        
        private LayerFactory() {}
        
        private bool CheckTypeIsLayer(Type type)
        {
            return type.IsClass && type.IsSubclassOf(typeof(ILayerHandler));
        }

        private void addLayerHandler(string title, Type type, Type exclusive = null)
        {
            List<(string, Type)> lst = _layerHandlers;
            
            if (exclusive != null)
            {
                if (!_gameStateLayerHandlers.ContainsKey(exclusive))
                    _gameStateLayerHandlers.Add(exclusive, new List<(string, Type)>());
                lst = _gameStateLayerHandlers[exclusive];
            }
            
            lst.Add((title, type));
        }

        private bool registerLayerHandler(string title, Type type, Type exclusive = null)
        {
            if (!CheckTypeIsLayer(type))
                return false;

            addLayerHandler(title, type, exclusive);

            return true;
        }

        public List<(string, Type)> GetAvailableLayerHandlers(Type gamestateType = null)
        {
            List<(string, Type)> lst = _layerHandlers.ToList();
            
            if (gamestateType != null && _gameStateLayerHandlers.ContainsKey(gamestateType))
                lst.AddRange(_gameStateLayerHandlers[gamestateType]);

            return lst;
        }

        /*public Type GetLayerHandlerType(string key)
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
        }*/

        public void Visit(List<Type> plugin)
        {
            foreach (Type typ in plugin.Where(CheckTypeIsLayer))
            {
                var title = typ.GetCustomAttribute<TitleAttribute>();

                if (title != null)
                {
                    var exclusive = typ.GetCustomAttribute<ExclusiveLayerHandlerAttribute>();
                    this.addLayerHandler(title.Title, typ, exclusive?.ExclusiveGameState);
                }
            }
        }
    }
}