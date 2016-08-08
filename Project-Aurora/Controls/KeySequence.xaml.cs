using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public List<Devices.DeviceKeys> List
        {
            get
            {
                if (Sequence == null)
                    Sequence = new Settings.KeySequence();

                return Sequence.keys;
            }
            set
            {
                if (Sequence == null)
                    Sequence = new Settings.KeySequence(value.ToArray());
                else
                    Sequence.keys = value;
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
                if(!value.Equals(Sequence))
                {
                    sequence_removeFromLayerEditor();
                }

                SetValue(SequenceProperty, value);

                sequence_updateToLayerEditor();

                if (allowListRefresh)
                {
                    this.keys_keysequence.Items.Clear();
                    foreach (var key in value.keys)
                        this.keys_keysequence.Items.Add(key);
                }

                this.sequence_freestyle_checkbox.IsChecked = (value.type == Settings.KeySequenceType.FreeForm ? true : false);

                if (SequenceUpdated != null)
                    SequenceUpdated(this, new EventArgs());
            }
        }

        public event EventHandler SequenceUpdated;

        public KeySequence()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void sequence_remove_keys_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.UIUtils.ListBoxRemoveSelected(this.keys_keysequence))
            {
                allowListRefresh = false;
                List = Utils.UIUtils.SequenceToList(this.keys_keysequence.Items);
                allowListRefresh = true;
            }
        }

        private void sequence_up_keys_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.UIUtils.ListBoxMoveSelectedUp(this.keys_keysequence))
            {
                allowListRefresh = false;
                List = Utils.UIUtils.SequenceToList(this.keys_keysequence.Items);
                allowListRefresh = true;
            }
        }

        private void sequence_down_keys_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.UIUtils.ListBoxMoveSelectedDown(this.keys_keysequence))
            {
                allowListRefresh = false;
                List = Utils.UIUtils.SequenceToList(this.keys_keysequence.Items);
                allowListRefresh = true;
            }
        }

        private void sequence_record_keys_Click(object sender, RoutedEventArgs e)
        {
            RecordKeySequence(RecordingTag, (sender as Button), this.keys_keysequence);
            allowListRefresh = false;
            List = Utils.UIUtils.SequenceToList(this.keys_keysequence.Items);
            allowListRefresh = true;
        }

        private void RecordKeySequence(string whoisrecording, Button button, ListBox sequence_listbox)
        {
            if (Global.key_recorder.IsRecording())
            {
                if (Global.key_recorder.GetRecordingType().Equals(whoisrecording))
                {
                    Global.key_recorder.StopRecording();

                    button.Content = "Assign Keys";

                    Devices.DeviceKeys[] recorded_keys = Global.key_recorder.GetKeys();

                    if (sequence_listbox.SelectedIndex > 0 && sequence_listbox.SelectedIndex < (sequence_listbox.Items.Count - 1))
                    {
                        int insertpos = sequence_listbox.SelectedIndex;
                        foreach (var key in recorded_keys)
                        {
                            sequence_listbox.Items.Insert(insertpos, key);
                            insertpos++;
                        }
                    }
                    else
                    {
                        foreach (var key in recorded_keys)
                            sequence_listbox.Items.Add(key);
                    }

                    Global.key_recorder.Reset();
                }
                else
                {
                    System.Windows.MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            }
            else
            {
                Global.key_recorder.StartRecording(whoisrecording);
                button.Content = "Stop Assigning";
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
            if (Sequence != null && IsInitialized && IsVisible)
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
            }
        }

        private void keys_keysequence_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(keys_keysequence.SelectedItems.Count <= 1)
            {
                this.sequence_up.IsEnabled = IsEnabled && true;
                this.sequence_down.IsEnabled = IsEnabled && true;
            }
            else
            {
                this.sequence_up.IsEnabled = IsEnabled && false;
                this.sequence_down.IsEnabled = IsEnabled && false;
            }
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member