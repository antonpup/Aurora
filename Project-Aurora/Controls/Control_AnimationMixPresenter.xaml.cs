using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_AnimationMixPresenter.xaml
    /// </summary>
    public partial class Control_AnimationMixPresenter : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ContextMixProperty = DependencyProperty.Register("ContextMix", typeof(AnimationMix), typeof(Control_AnimationMixPresenter));

        public AnimationMix ContextMix
        {
            get
            {
                return (AnimationMix)GetValue(ContextMixProperty);
            }
            set
            {
                SetValue(ContextMixProperty, value);

                stkPanelTracks.Children.Clear();

                foreach(var track in value.GetTracks())
                {
                    Control_AnimationTrackPresenter newTrack = new Control_AnimationTrackPresenter() { ContextTrack = track.Value };

                    stkPanelTracks.Children.Add(newTrack);
                    stkPanelTracks.Children.Add(new Separator());
                }
            }
        }

        public Control_AnimationMixPresenter()
        {
            InitializeComponent();
        }
    }
}
