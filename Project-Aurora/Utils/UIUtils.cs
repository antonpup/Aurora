using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Utils
{
    public static class UIUtils
    {
        public static void SetSingleKey(TextBlock key_destination, List<DeviceKeys> keyslist, int position)
        {
            if (keyslist.Count > position)
                key_destination.Text = Enum.GetName(typeof(DeviceKeys), keyslist[position]);
            else
                key_destination.Text = Enum.GetName(typeof(DeviceKeys), DeviceKeys.NONE);
        }

        public static List<Devices.DeviceKeys> SequenceToList(ItemCollection items)
        {
            List<Devices.DeviceKeys> newsequence = new List<Devices.DeviceKeys>();

            foreach (Devices.DeviceKeys key in items)
            {
                newsequence.Add(key);
            }

            return newsequence;
        }

        public static bool ListBoxMoveSelectedUp(ListBox list)
        {
            if (list.Items.Count > 0 && list.SelectedIndex > 0)
            {
                int selected_index = list.SelectedIndex;
                var saved = list.Items[selected_index];
                list.Items[selected_index] = list.Items[selected_index - 1];
                list.Items[selected_index - 1] = saved;
                list.SelectedIndex = selected_index - 1;

                list.ScrollIntoView(list.Items[selected_index - 1]);
                return true;
            }

            return false;
        }

        public static bool ListBoxMoveSelectedDown(ListBox list)
        {
            if (list.Items.Count > 0 && list.SelectedIndex < (list.Items.Count - 1) && list.SelectedIndex >= 0)
            {
                int selected_index = list.SelectedIndex;
                var saved = list.Items[selected_index];
                list.Items[selected_index] = list.Items[selected_index + 1];
                list.Items[selected_index + 1] = saved;
                list.SelectedIndex = selected_index + 1;

                list.ScrollIntoView(list.Items[selected_index + 1]);
                return true;
            }

            return false;
        }

        public static bool ListBoxRemoveSelected(ListBox list)
        {

            if(list.SelectedItems.Count == 1)
            {
                if (list.Items.Count > 0 && list.SelectedIndex >= 0)
                {
                    int selected = list.SelectedIndex;
                    list.Items.RemoveAt(selected);

                    if (list.Items.Count > selected)
                        list.SelectedIndex = selected;
                    else
                        list.SelectedIndex = (list.Items.Count - 1);

                    if (list.SelectedIndex > -1)
                        list.ScrollIntoView(list.Items[list.SelectedIndex]);

                    return true;
                }

                return false;
            }
            else
            {
                bool isremoved = false;

                while (list.SelectedItems.Count > 0)
                {
                    list.Items.Remove(list.SelectedItems[0]);

                    isremoved = true;
                }

                return isremoved;
            }
        }
    }
}
