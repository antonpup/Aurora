using System;
using System.Windows;
using System.Windows.Input;

namespace Aurora.Utils {

	/// <summary>
	/// Behaviour that can be attached to any <see cref="UIElement"/> to provide an event that fires when the user clicks and drags the element
	/// with their left mouse button. Abstracts away the code to store initial mouse position and calculate different to current mouse position.
	/// </summary>
	public static class DragBehaviour {

		#region MouseDownStart Property
		// Property that is attached to an element and used to determine the location the mouse when down relative to the element to decide whether or not to
		// initiate a drag. This is private as there is no need for this to be accessed outside of the StartDrag property.
		private static Point? GetMouseDownStart(DependencyObject obj) => (Point?)obj.GetValue(MouseDownStartProperty);
		private static void SetMouseDownStart(DependencyObject obj, Point? value) => obj.SetValue(MouseDownStartProperty, value);
		private static readonly DependencyProperty MouseDownStartProperty =
			DependencyProperty.RegisterAttached("MouseDownStart", typeof(Point?), typeof(DragBehaviour), new PropertyMetadata(null));
		#endregion

		#region StartDrag Property
		/// <summary>Sets the event that will be called when the target element is pressed and the mosue moved a minimum distance as set by the system parameters.</summary>
		/// <param name="obj">The target element which the event should be attached to.</param>
		public static StartDragEventHandler GetStartDrag(DependencyObject obj) => (StartDragEventHandler)obj.GetValue(StartDragProperty);
		/// <summary>Sets the event that will be called when the target element is pressed and the mosue moved a minimum distance as set by the system parameters.</summary>
		/// <param name="obj">The target element which the event should be attached to. Has no effect on DependencyProperties that are not <see cref="UIElement"/>s.</param>
		/// <param name="value">The event that will be called when the drag is started.</param>
		public static void SetStartDrag(DependencyObject obj, StartDragEventHandler value) => obj.SetValue(StartDragProperty, value);

		/// <summary>This property represents an event that is fired when the drag is initiated.</summary>
		public static readonly DependencyProperty StartDragProperty =
			DependencyProperty.RegisterAttached("StartDrag", typeof(StartDragEventHandler), typeof(DragBehaviour), new UIPropertyMetadata(null, StartDragPropertyChanged));

		/// <summary>Callback that will update the event handlers on the target when the StartDrag property is changed.</summary>
		private static void StartDragPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			// Can only add MouseEvents onto UIElements.
			if (!(sender is UIElement el)) return;

			// If there is not an existing DragEvent, but there is one being set, add the relevant event listeners.
			if (e.OldValue == null && e.NewValue != null) {
				el.MouseLeftButtonDown += DependencyTarget_MouseLeftButtonDown;
				el.MouseMove += DependencyTarget_MouseMove;
				el.MouseLeftButtonUp += DependencyTarget_MouseLeftButtonUp;

				// If there is an existing DragEvent, but the new one is now null, remove the relevant event listeners.
			} else if (e.OldValue != null && e.NewValue == null) {
				el.MouseLeftButtonDown -= DependencyTarget_MouseLeftButtonDown;
				el.MouseMove -= DependencyTarget_MouseMove;
			}
		}
		#endregion

		#region Events
		/// <summary>LeftButtonDown event for any elements who have the behaviour attached. Will set that object's <see cref="MouseDownStartProperty" /> to the
		/// current position of the mouse relative to the sending object.</summary>
		private static void DependencyTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			var trg = (UIElement)sender;
			e.Handled = true;
			trg.CaptureMouse(); // Capture mouse to detect mouse movement off the element
			SetMouseDownStart(trg, e.GetPosition((IInputElement)sender));
		}

		/// <summary>MouseMove event from any elements who have the behaviour attached. Will check if the mouse went down on this element and then if the user
		/// has dragged the mouse atleast the distance set by the system parameters. If both these cases are true, fires a single StartDrag event.</summary>
		private static void DependencyTarget_MouseMove(object sender, MouseEventArgs e) {
			var @do = (UIElement)sender;
			var p = GetMouseDownStart(@do);
			var delta = e.GetPosition((IInputElement)sender) - p; // Calculate the distance between the current mouse position and the initial mouse down position
																  // Note that this is a Vector? because one of the operands (GetMouseDownStart) is a Point?, so if the Point is null, this cascades to the Vector.

			// If this element is being dragged (delta will be null if not), then check the delta is atleast as much as the system parameters
			if (delta != null && (Math.Abs(delta.Value.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(delta.Value.Y) > SystemParameters.MinimumVerticalDragDistance)) {
				@do.ReleaseMouseCapture();
				GetStartDrag(@do)?.Invoke(sender, e, p.Value); // Invoke the StartDrag event assigned to the DependecyObject
				SetMouseDownStart(@do, null); // Set the start mouse point to null to prevent this StartDrag being called again for this drag.
			}
		}

		/// <summary>LeftButtonUp handler for any elements with the behaviour attached. Will set that object's <see cref="MouseDownStartProperty"/> to null to prevent
		/// the <see cref="DependencyTarget_MouseMove(object, MouseEventArgs)"/> event from running and triggering a StartDrag event.</summary>
		private static void DependencyTarget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			var trg = (UIElement)sender;
			trg.ReleaseMouseCapture();
			SetMouseDownStart(trg, null);
		}
		#endregion
	}

	/// <summary>Event handler that is called when the StartDrag event is triggered.</summary>
	/// <param name="sender">The object that initiated the event.</param>
	/// <param name="e">The event of the most recent MouseMove event that caused the StartDrag event to fire.</param>
	/// <param name="initialPoint">The initial mouse location relative to the target when the mouse was originally pressed.</param>
	public delegate void StartDragEventHandler(object sender, MouseEventArgs e, Point initialPoint);
}