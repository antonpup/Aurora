using System;
using System.Collections.Generic;
using Aurora.Applications.Application;
using Aurora.EffectsEngine;

namespace Aurora.Applications.Layers
{
    public class Logic{}
    
    public interface ILayer
    {
        //TODO: Finalise Logic implementation
        Logic Logic { get; set; }
        
        //TODO: Finalise PostProcess implementation
        List<(string, IPostProcess)> PostProcess { get; set; }
        
        string Name { get; set; }
        
        bool IsEnabled { get; set; }
        
        IEffectLayer Render(GameState gs);
    }
    
    public class Layer : ILayer
    {
        public Logic Logic { get; set; }
        public List<(string, IPostProcess)> PostProcess { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public ILayerHandler LayerHandler { get; set; }
        
        public IEffectLayer Render(GameState gs)
        {
            if (!IsEnabled)
                return null;
            
            //TODO: Check/Apply logic
            
            var layer = LayerHandler.Render(gs);
            
            //TODO: PostProcess

            return layer;
        }
    }

    public class LayerGroup : ILayer
    {
        public Logic Logic { get; set; }
        public List<(string, IPostProcess)> PostProcess { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public List<IEffectLayer> LayerHandlers { get; set; }
        
        public IEffectLayer Render(GameState gs)
        {
            if (!IsEnabled)
                return null;
            
            //TODO: Check/Apply logic
            
            //TODO: Render LayerHandlers and merge
            
            //TODO: PostProcess

            return null;
        }
    }
}