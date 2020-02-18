using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora.Controls
{
    public partial class KeySequence : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(UserControl));

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty RecordingTagProperty = DependencyProperty.Register("RecordingTag", typeof(string), typeof(UserControl));

        public string RecordingTag
        {
            get
            {
                return (string)GetValue(RecordingTagProperty);
            }
            set
            {
                SetValue(RecordingTagProperty, value);
            }
        }

        private bool allowListRefresh = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty SequenceProperty = DependencyProperty.Register("Sequence", typeof(Settings.KeySequence), typeof(UserControl));

        public Settings.KeySequence Sequence
        {
            get
            {
                return (Settings.KeySequence)GetValue(SequenceProperty);
            }
            set
            {
                if (value == null)
                    value = new Settings.KeySequence();

                if (!value.Equals(Sequence))
                {
                    sequence_removeFromLayerEditor();
                }

                SetValue(SequenceProperty, value);

                sequence_updateToLayerEditor();

                if (allowListRefresh)
                {
                    SelectedDeviceKeyList.Clear();
                    foreach (var key in value.keys)
                        SelectedDeviceKeyList.Add(key);
                }

                this.sequence_freestyle_checkbox.IsChecked = (value.type == Settings.KeySequenceType.FreeForm ? true : false);

                if (SequenceUpdated != null)
                    SequenceUpdated(this, new EventArgs());
            }
        }

        public IEnumerable<DeviceKey> SelectedItems => keys_keysequence.SelectedItems.Cast<DeviceKey>();

        private int SelectedItemIndex => keys_keysequence.SelectedIndex;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty FreestyleEnabledProperty = DependencyProperty.Register("FreestyleEnabled", typeof(bool), typeof(UserControl));

        public bool FreestyleEnabled
        {
            get
            {
                return (bool)GetValue(FreestyleEnabledProperty);
            }
            set
            {
                SetValue(FreestyleEnabledProperty, value);

                this.sequence_freestyle_checkbox.IsEnabled = value;
                this.sequence_freestyle_checkbox.ToolTip = (value ? null : "Freestyle has been disabled.");
                    
            }
        }

        /// <summary>Fired whenever the KeySequence object is changed or re-created. Does NOT trigger when keys are changed.</summary>
        public event EventHandler SequenceUpdated;
        /// <summary>Fired whenever keys are changed.</summary>
        public event EventHandler SequenceKeysChange;
        public event SelectionChangedEventHandler SelectionChanged;
        public ObservableCollection<DeviceKey> SelectedDeviceKeyList { get; set; }

        public List<DeviceKey> SelectedDeviceKeys => SelectedDeviceKeyList.ToList();


        public KeySequence()
        {
            InitializeComponent();
            this.DataContext = this;
            SelectedDeviceKeyList = new ObservableCollection<DeviceKey>();
        }

        private void sequence_remove_keys_Click(object sender, RoutedEventArgs e)
        {
            var selectedKeys = SelectedItems.ToList();
            foreach (var item in selectedKeys)
            {
                if(SelectedDeviceKeyList.Contains(item))
                    SelectedDeviceKeyList.Remove(item);          
            }
        }

        private void sequence_up_keys_Click(object sender, RoutedEventArgs e)
        {
            var selectedKey = SelectedDeviceKeyList[SelectedItemIndex];
            var selectedIndex = SelectedItemIndex;
            SelectedDeviceKeyList[selectedIndex] = SelectedDeviceKeyList[selectedIndex - 1];
            SelectedDeviceKeyList[selectedIndex - 1] = selectedKey;
            keys_keysequence.SelectedItem = selectedKey;
        }

        private void sequence_down_keys_Click(object sender, RoutedEventArgs e)
        {
            var selectedKey = SelectedDeviceKeyList[SelectedItemIndex];
            var selectedIndex = SelectedItemIndex;
            SelectedDeviceKeyList[selectedIndex] = SelectedDeviceKeyList[selectedIndex + 1];
            SelectedDeviceKeyList[selectedIndex + 1] = selectedKey;
            keys_keysequence.SelectedItem = selectedKey;
        }

        private void btnReverseOrder_Click(object sender, RoutedEventArgs e)
        {
            int totalCount = SelectedDeviceKeyList.Count;
            for (int i = totalCount - 1; i > 0; i--)
            {
                DeviceKey key = SelectedDeviceKeyList[totalCount - 1];
                SelectedDeviceKeyList.RemoveAt(totalCount - 1);
                SelectedDeviceKeyList.Insert((totalCount - 1) - i, key);
            }
        }

        private void sequence_record_keys_Click(object sender, RoutedEventArgs e)
        {
            if (Global.key_recorder.IsRecording())
            {
                if (Global.key_recorder.GetRecordingType().Equals(RecordingTag))
                {
                    Global.key_recorder.StopRecording();

                    (sender as Button).Content = "Assign Keys";

                    DeviceKey[] recorded_keys = Global.key_recorder.GetKeys();

                    if (SelectedItemIndex > 0 && SelectedItemIndex < (SelectedDeviceKeyList.Count - 1))
                    {
                        int insertpos = SelectedItemIndex;
                        foreach (var key in recorded_keys)
                        {
                            SelectedDeviceKeyList.Insert(insertpos, key);
                            insertpos++;
                        }
                    }
                    else
                    {
                        foreach (var key in recorded_keys)
                            SelectedDeviceKeyList.Add(key);
                    }
                    Global.key_recorder.Reset();
                    Sequence.keys = SelectedDeviceKeyList.ToList();
                    //SequenceKeysChange.Invoke(this, e);
                }
                else
                {
                    MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            }
            else
            {
                Global.key_recorder.StartRecording(RecordingTag);
                (sender as Button).Content = "Stop Assigning";
            }
        }

        private void sequence_freestyle_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.KeySequence seq = Sequence;
            if (seq != null && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
            {
                seq.type = ((sender as CheckBox).IsChecked.Value ? Settings.KeySequenceType.FreeForm : Settings.KeySequenceType.Sequence);
                Sequence = seq;

                sequence_updateToLayerEditor();
            }
        }

        public void sequence_updateToLayerEditor()
        {
            if (Sequence != null && IsInitialized && IsVisible && IsEnabled)
            {
                if (Sequence.type == Settings.KeySequenceType.FreeForm)
                {
                    Sequence.freeform.ValuesChanged += freeform_updated;
                    LayerEditor.AddKeySequenceElement(Sequence.freeform, Color.FromRgb(255, 255, 255), Title);
                }
                else
                {
                    Sequence.freeform.ValuesChanged -= freeform_updated;
                    LayerEditor.RemoveKeySequenceElement(Sequence.freeform);
                }
            }
        }

        private void freeform_updated(Settings.FreeFormObject newfreeform)
        {
            if(newfreeform != null)
            {
                Sequence.freeform = newfreeform;

                if (SequenceUpdated != null)
                    SequenceUpdated(this, new EventArgs());
            }
        }

        private void sequence_removeFromLayerEditor()
        {
            if (Sequence != null && IsInitialized)
            {
                if (Sequence.type == Settings.KeySequenceType.FreeForm)
                {
                    Sequence.freeform.ValuesChanged -= freeform_updated;
                    LayerEditor.RemoveKeySequenceElement(Sequence.freeform);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            sequence_updateToLayerEditor();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            sequence_removeFromLayerEditor();
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is bool)
            {
                this.keys_keysequence.IsEnabled = (bool)e.NewValue;
                this.sequence_record.IsEnabled = (bool)e.NewValue;
                this.sequence_up.IsEnabled = (bool)e.NewValue;
                this.sequence_down.IsEnabled = (bool)e.NewValue;
                this.sequence_remove.IsEnabled = (bool)e.NewValue;
                this.sequence_freestyle_checkbox.IsEnabled = (bool)e.NewValue;
                //this.sequence_freestyle_checkbox.IsEnabled = (bool)e.NewValue && FreestyleEnabled;

                if ((bool)e.NewValue)
                    sequence_updateToLayerEditor();
                else
                    sequence_removeFromLayerEditor();
            }
        }

        private void keys_keysequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.sequence_up.IsEnabled = false;
            this.sequence_down.IsEnabled = false;

            if (SelectedItems.Count() == 1)
            {
                if (SelectedItemIndex != 0)
                    this.sequence_up.IsEnabled = IsEnabled && true;
                if (SelectedItemIndex != SelectedDeviceKeyList.Count() - 1)
                    this.sequence_down.IsEnabled = IsEnabled && true;
            }

            // Bubble the selection changed event
            SelectionChanged?.Invoke(this, e);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool)
            {
                if ((bool)e.NewValue)
                {
                    sequence_updateToLayerEditor();
                    if (Sequence != null)
                    {
                        //this.keys_keysequence.InvalidateVisual();
                        SelectedDeviceKeyList.Clear();
                        foreach (var key in Sequence.keys)
                            SelectedDeviceKeyList.Add(key);
                    }
                }
                else
                    sequence_removeFromLayerEditor();
            }
                
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member