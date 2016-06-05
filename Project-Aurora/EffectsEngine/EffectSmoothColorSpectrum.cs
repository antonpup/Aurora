using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine
{
    public class SmoothColorSpectrum
    {
        private List<Color> steps = new List<Color>();
        private int current_step = 0;

        public SmoothColorSpectrum(Color initial, Color final)
        {
            steps = StepsBetweenColors(initial, final);
        }

        public Color NextStep()
        {
            current_step++;
            return steps[current_step - 1];
        }

        public bool IsInProgress()
        {
            return current_step != 0;
        }

        public bool HasFinished()
        {
            return current_step == steps.Count;
        }

        private List<Color> StepsBetweenColors(Color A, Color B)
        {
            List<Color> colors = new List<Color>();

            List<int> redChanges = CalculateSteps(A.R, B.R);
            List<int> greenChanges = CalculateSteps(A.G, B.G);
            List<int> blueChanges = CalculateSteps(A.B, B.B);
            List<int> alphaChanges = CalculateSteps(A.A, B.A);

            int largestChangesCount = 0;

            if (redChanges.Count > largestChangesCount)
                largestChangesCount = redChanges.Count;
            if (greenChanges.Count > largestChangesCount)
                largestChangesCount = greenChanges.Count;
            if (blueChanges.Count > largestChangesCount)
                largestChangesCount = blueChanges.Count;
            if (alphaChanges.Count > largestChangesCount)
                largestChangesCount = alphaChanges.Count;

            int percent = 0;

            for(int step = 0; step < largestChangesCount; step++)
            {
                percent = ( 100 * step) / largestChangesCount;

                int rValue = (percent * redChanges.Count) / 100;
                int gValue = (percent * greenChanges.Count) / 100;
                int bValue = (percent * blueChanges.Count) / 100;
                int aValue = (percent * alphaChanges.Count) / 100;

                colors.Add(Color.FromArgb(alphaChanges[aValue], redChanges[rValue], greenChanges[gValue], blueChanges[bValue]));
            }

            return colors;
        }

        private List<int> CalculateSteps(int initial, int final)
        {
            int threshold = 2;
            int step_value = 1;

            int RDirection = (initial > final ? -1 : 1);
            int RDifference = (initial > final ? initial - final : final - initial);
            int RDiff_total = 0;
            int RTotalSteps = (int)Math.Ceiling((double)RDifference / (double)step_value);

            List<int> RSteps = new List<int>();

            for (RDiff_total = 0; RDifference + RDiff_total > threshold; RDiff_total += RDirection * step_value)
                RSteps.Add(initial + RDiff_total + RDirection * step_value);

            //Less than threshhold
            RSteps.Add(final);

            return RSteps;
        }
    }
}
