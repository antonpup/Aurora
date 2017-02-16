using Aurora.EffectsEngine.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine
{
    public class EffectColorFunction
    {
        private EffectFunction function;
        private ColorSpectrum colorspectrum;
        private int size = 1;
        private EffectPoint origin;

        public EffectColorFunction(EffectFunction function, ColorSpectrum colorspectrum, int size = 1)
        {
            this.function = function;
            this.colorspectrum = colorspectrum;
            this.size = size;
            this.origin = function.GetOrigin();
        }


        public Tuple<EffectPoint, Color>[] getPointMap()
        {
            //List<Tuple<EffectPoint, EffectColor>> points = new List<Tuple<EffectPoint, EffectColor>>();

            List<EffectPoint[]> points = new List<EffectPoint[]>();

            if(function is EffectCircle)
            {
                for (float t = 0.0f; t < Math.PI * 2.0f; t += 0.025f)
                {
                    EffectPoint point = function.GetPoint(t);

                    HashSet<EffectPoint> newpts = new HashSet<EffectPoint>();

                    if (!point.IsOutOfRange())
                    {
                        newpts.Add( new EffectPoint(point.X, point.Y) );

                        for (int offset = 0; offset < size; offset++)
                        {
                            newpts.Add(new EffectPoint(point.X + offset, point.Y));
                            newpts.Add(new EffectPoint(point.X, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y));
                            newpts.Add(new EffectPoint(point.X, point.Y - offset));
                            newpts.Add(new EffectPoint(point.X + offset, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X + offset, point.Y - offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y - offset));
                        }
                    }

                    if (newpts.Count != 0)
                        points.Add(newpts.ToArray());
                }
            }
            else
            {
                for (float x = 0.0f; x < Effects.canvas_width; x += 1.0f)
                {
                    EffectPoint point = function.GetPoint(x);

                    List<EffectPoint> newpts = new List<EffectPoint>();

                    if (!point.IsOutOfRange())
                    {
                        newpts.Add(point);

                        for (int offset = 0; offset < size; offset++)
                        {
                            newpts.Add(new EffectPoint(point.X + offset, point.Y));
                            newpts.Add(new EffectPoint(point.X, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y));
                            newpts.Add(new EffectPoint(point.X, point.Y - offset));
                            newpts.Add(new EffectPoint(point.X + offset, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X + offset, point.Y - offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y + offset));
                            newpts.Add(new EffectPoint(point.X - offset, point.Y - offset));
                        }
                    }

                    if (newpts.Count != 0)
                        points.Add(newpts.ToArray());
                }
            }

            List<Tuple<EffectPoint, Color>> points_ret = new List<Tuple<EffectPoint, Color>>();

            for(int pt_id = 0; pt_id < points.Count; pt_id++)
            {
                Color color = colorspectrum.GetColorAt((float)pt_id / points.Count);

                foreach (EffectPoint pt in points[pt_id])
                {
                    points_ret.Add(new Tuple<EffectPoint, Color>(pt, color));
                }
                
            }

            return points_ret.ToArray();
        }


    }
}
