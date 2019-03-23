using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aurora.Controls {

    /// <summary>
    /// A special ListBox that allows the items that make up the listbox to be dragged up and down.
    /// <para>It is recommended to use this in conjunction with an <see cref="ObservableCollection{T}"/>.</para>
    /// </summary>
    public class ReorderableListBox : ListBox {
        
        /// <summary>A reference to the items panel that is used for the item list arrangement.</summary>
        private ReorderableListBoxPanel panel;

        /// <summary>A reference to the item currently being dragged by the user. Will be null if no item is being dragged</summary>
        private ListBoxItem draggedItem;

        /// <summary>The relative point where the users mouse originally was pressed. Used to assert a minimum distance before the drag operation begins</summary>
        private Point startDragPoint;

        /// <summary>The height of the items in this list box. Will be set by the list box when the user starts dragging an item.
        /// Used for drop index detection. Will not work properly if items are different heights.</summary>
        private double itemHeight = 0;

        /// <summary>Get the ListBox style and create a new ReorderableListBox style based on it.</summary>
        /// <remarks>For whatever reason the style isn't inherited from the ListBox, even though we are extending it.</remarks>
        private static Style listBoxStyle = new Style(typeof(ReorderableListBox), Application.Current.TryFindResource(typeof(ListBox)) as Style);

        /// <summary>Get the style that is used on ListBoxItems so that when we create a new style for this ReorderableListBox, we can use
        /// this as a base for the items here.</summary>
        private static Style listboxItemStyle = Application.Current.TryFindResource(typeof(ListBoxItem)) as Style;


        public ReorderableListBox() : base() {
            // We want to use the custom ReorderableListBoxPanel with out ReorderableListBoxes
            ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(ReorderableListBoxPanel)));

            // When the template is executed and a panel created, we want to store a reference to the panel so we can manipulate it.
            ItemsPanel.VisualTree.AddHandler(LoadedEvent, new RoutedEventHandler((sender, e) => panel = (ReorderableListBoxPanel)sender));

            AllowDrop = true; // Enable dropping on this listbox
            MouseLeftButtonUp += (sender, e) => draggedItem = null; // When the user releases the mount, ensure dragged item is reset
            MouseMove += ReorderableListBox_MouseMove;
            DragOver += ReorderableListBox_DragOver; // Preview event to update the list box items's arrangement (e.g. the gap and "floating" element)
            Drop += ReorderableListBox_Drop; // Drop event for when the user releases the mouse

            Style = listBoxStyle;

            // Create a style that assigns the preview mouse event to the children of the listbox
            var itemContainerStyle = new Style(typeof(ListBoxItem), listboxItemStyle);
            itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ReorderableListBoxItem_PreviewMouseLeftButtonDown)));
            ItemContainerStyle = itemContainerStyle;
        }
        
        #region DragElementTag
        /// <summary>
        /// This property is used to specify which element inside the ItemTemplate can trigger the drag-drop reordering functionality of this list box.
        /// If null, any element will trigger it. If provided a value, the `OriginalSource` of the mouse click event will be checked for a matching tag.
        /// <para>For example, if a icon with a drag image was in the template, you could apply "Dragger" as the tag and set this DragElementTag to also
        /// be "Dragger" and then only that icon would be able to trigger the drag.</para>
        /// </summary>
        public object DragElementTag {
            get => GetValue(DragElementTagProperty);
            set => SetValue(DragElementTagProperty, value);
        }
        
        public static readonly DependencyProperty DragElementTagProperty =
            DependencyProperty.Register("DragElementTag", typeof(object), typeof(ReorderableListBox), new PropertyMetadata(null));
        #endregion

        /// <summary>
        /// Event that is fired when the user clicks on a item in the ListBox. Stores the clicked item and the relative mouse location.
        /// </summary>
        private void ReorderableListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e) {
            // Guard to check if the sender of the event is a ListBox item (no reason it shouldn't be though)
            // Also check that if the developer has specified a "DragElementTag", the original source of the event (the actual clicked element, even if it is
            // inside the element with the attached listener, so in this case a particular object in the item template) has a tag that matches the provided one.
            if (sender is ListBoxItem item && (DragElementTag == null || DragElementTag.Equals((e.OriginalSource as FrameworkElement)?.Tag))) {
                startDragPoint = e.GetPosition(this);
                draggedItem = item;
            }
        }

        /// <summary>
        /// Event that is fired when the mouse moves over the listbox. Checks if the user selected an item (and still has the mouse down since
        /// draggedItem becomes null when mouse released) and if the user has moved the mouse a certain distance from the original start mouse
        /// down point then start the drag-drop operation.
        /// </summary>
        private void ReorderableListBox_MouseMove(object sender, MouseEventArgs e) {
            if (draggedItem != null) {
                var delta = e.GetPosition(this) - startDragPoint; // Calculate distacne between current mouse point and original mouse down point
                if (Math.Abs(delta.X) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(delta.Y) >= SystemParameters.MinimumVerticalDragDistance) // Check it's moved a certain distance
                    BeginDragDrop();
            }
        }

        /// <summary>
        /// Start the drag-drop operation on the current `draggedItem`.
        /// </summary>
        private void BeginDragDrop() {
            draggedItem.IsSelected = true;
            panel.DraggedIndex = Items.IndexOf(draggedItem.DataContext); // Tell the panel which item should start "floating" at the cursor
            itemHeight = draggedItem.DesiredSize.Height; // Store the item's height for use in the drop index calculations

            // Actually start a drag-drop operation. Although this could be done without a drag-drop, this makes it easier since
            // this will handle mouse releases outside the Aurora window (manual detection of this is messy).
            DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);

            // The `DoDragDrop` is blocking, so this executes after the user has dropped the item
            draggedItem = null; // No longer dragging/dropping an item
            panel.DropIndex = panel.DraggedIndex = -1; // Also clear the panel's indexes so it draws as if everything is normal
        }

        /// <summary>
        /// This event fires continuously while the user is dragging something over this list box.
        /// Handles passing new data to the panel so that it can arrange the items to help the user understand where the item will be dropped.
        /// </summary>
        private void ReorderableListBox_DragOver(object sender, DragEventArgs e) {
            // Guard to ensure that the drag originated from this list box (if from elsewhere, draggedItem will be null)
            if (draggedItem == null) return;

            var y = e.GetPosition(this).Y; // Y position of the mouse relative to `this` listbox
            panel.DropIndex = CalculateDropIndex(y); // Calculate the index that the item would be inserted if the user dropped here (and pass to panel for rendering)
            panel.DraggedY = y; // Also pass the relative Y coordinate to the panel, also for rendering
        }

        /// <summary>
        /// Event that is fired when the user drops an item on the list box.
        /// </summary>
        private void ReorderableListBox_Drop(object sender, DragEventArgs e) {
            // Guard to ensure that the drag originated from this list box (if from elsewhere, draggedItem will be null)
            if (draggedItem == null) return;

            var itemData = draggedItem.DataContext; // The actual data for the item that the user dragged is contained within the DataContext of draggedItem
            var oldIndex = Items.IndexOf(draggedItem.DataContext); // Get the old index of the item, based on the current index of the dragged item
            var newIndex = CalculateDropIndex(e.GetPosition(this).Y); // Get the new index of item, based on the user's mouse's Y location

            // If the new index is after the old index, we must subtract one from it since it will change when we remove the item from the old index location.
            if (oldIndex < newIndex) newIndex--;

            // Do the actual move in the collection
            ((IList)ItemsSource).RemoveAt(oldIndex);
            ((IList)ItemsSource).Insert(newIndex, itemData);

            // Re-select this item (removing it and re-adding it will deselect it)
            SelectedItem = itemData;
        }

        /// <summary>
        /// Calculates the index that an item will be dropped in if the user was to drop it at the given Y location (relative to this list box).
        /// </summary>
        private int CalculateDropIndex(double y) {
            var idx = (int)(y / itemHeight); // We're making the assumtion that all items are the same height, which they usually are.
            if (draggedItem != null && idx >= panel.DraggedIndex) idx++; // During dragging operations, since the dragged item is no longer part of the layout, if the detected index is AFTER the dragged item, treat it as 1 higher than it would normally be
            return Math.Max(Math.Min(idx, Items.Count), 0); // Ensure the returned value is within valid insert ranges (no more than item length and no less than 0)
        }
    }



    /// <summary>
    /// Specialised panel that exposes a few properties to help render a reorderable list box. This panel will show items
    /// top-to-bottom stacked on top of one another, but if non-negative, will show a blank space at the DropIndex position
    /// and make the DraggedIndex'th item float at the DraggedY position.
    /// </summary>
    class ReorderableListBoxPanel : Panel {

        /// <summary>The index of the location which the user is currently dropping into.</summary>
        public int DropIndex { get => dropIndex; set { dropIndex = value; InvalidateArrange(); } }
        private int dropIndex = -1;

        /// <summary>The index of the item that the user has started dragging</summary>
        public int DraggedIndex { get => draggedIndex; set { draggedIndex = value; InvalidateArrange(); } }
        private int draggedIndex = -1;

        /// <summary>The Y position of the item the user is dragging</summary>
        public double DraggedY { get => draggedY; set { draggedY = value; InvalidateArrange(); } }
        private double draggedY = 0;


        // This method is first called during the first pass by the parent to see how much space this element wants
        protected override Size MeasureOverride(Size availableSize) {
            // This method is based off code at https://www.codeproject.com/Articles/15705/FishEyePanel-FanPanel-Examples-of-custom-layout-pa
            if (!double.IsInfinity(availableSize.Width) && !double.IsInfinity(availableSize.Height))
                return availableSize;

            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);
            var ideal = new Size();
            foreach (UIElement child in Children) {
                child.Measure(size); // Measure children, so we have access to their desired size
                ideal.Height += child.DesiredSize.Height; // Ideal height is all the child ideal heights
                ideal.Width = Math.Max(child.DesiredSize.Width, ideal.Width); // Ideal width is the widest child
            }

            return ideal;
        }

        // After MeasureOverride, this method is called during the second pass by the parent, telling us how much space the element gets
        protected override Size ArrangeOverride(Size finalSize) {
            double y = 0; int i = 0;
            foreach (UIElement child in Children) {
                // If the index we're currently at is the DropIndex, insert a space before it.
                // E.G. at DropIndex 1, we want to insert space between the 0th and 1st children.
                if (i == DropIndex) y += Children[DraggedIndex].DesiredSize.Height;

                // Place this child at the given Y position (or float it if this child is the one being dragged as indicated by the DraggedIndex)
                child.Arrange(new Rect(0, i == draggedIndex ? (DraggedY - child.DesiredSize.Height / 2) : y, child.DesiredSize.Width, child.DesiredSize.Height));

                // If this item wasn't the "floating" dragged element, add it's height to the Y position counter
                if (i != DraggedIndex) y += child.DesiredSize.Height;

                i++;
            }
            return finalSize;
        }
    }
}
