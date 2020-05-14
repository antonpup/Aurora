using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// A control that can be used to present the controls of an IEvaluatable to the user, allowing them to edit the parameters.
    /// <para>Can set the type of value the presenter accepts using the <see cref="EvalType"/> dependency property.</para>
    /// </summary>
    public partial class Control_EvaluatablePresenter : UserControl {

        public Control_EvaluatablePresenter() {
            InitializeComponent();
        }

        /// <summary>Event that fires when a new evaluatable replaces the current one. Note that this only fires when the user replaces
        /// the evaluatable by dropping a new one onto the presenter, not when it is changed by code.</summary>
        public event EventHandler<ExpressionChangeEventArgs> ExpressionChanged;


        #region Expression Property
        public IEvaluatable Expression {
            get => (IEvaluatable)GetValue(ExpressionProperty);
            set => SetValue(ExpressionProperty, value);
        }

        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register("Expression", typeof(IEvaluatable), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnExpressionChange));

        private static void OnExpressionChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            if (eventArgs.NewValue == null || eventArgs.NewValue.Equals(eventArgs.OldValue)) return;
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            var expr = (IEvaluatable)eventArgs.NewValue;
            control.EvaluatableControl.Content = expr?.GetControl();
        }
        #endregion

        #region EvalType Property
        /// <summary>
        /// The subtype of evaluatable to restrict the user to (e.g. specifying bool would indicate IEvaluatable&lt;bool&gt;).<para/>
        /// Do NOT provide a type <c>IEvaluatable&lt;T&gt;</c> to this.
        /// </summary>
        public Type EvalType {
            get => (Type)GetValue(EvalTypeProperty);
            set => SetValue(EvalTypeProperty, value);
        }
        public static readonly DependencyProperty EvalTypeProperty =
            DependencyProperty.Register("EvalType", typeof(Type), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceEvalType));

        // Coerce any numeric types into double
        private static Type[] evalTypeNumerics = new[] { typeof(int), typeof(long), typeof(short), typeof(float), typeof(decimal) };
        private static object CoerceEvalType(DependencyObject d, object value) =>
            evalTypeNumerics.Contains(value as Type) ? typeof(double) : value;
        #endregion

        /// <summary>Write-only property to set the highlight status of this presenter.</summary>
        protected bool Highlighted {
            set => Background = value ? Brushes.Yellow : Brushes.Transparent;
        }

        #region Drag Handlers
        /// <summary>
        /// Method that handles when the user is dragging something and their mouse enters this presenter. If the dragged data is accepted
        /// by this presenter, it's visual state will change to indicate to the user it will be accepted.
        /// </summary>
        protected override void OnDragEnter(DragEventArgs e) {
            // When the user is dragging something over this presenter, check to see if it can be accepted (if the evaluatable type of the
            // dragged data matches the type specified in the property).
            if (TryGetData(e.Data, out _, out var source) && !source.IsParentOf(this)) {
                // If so, highlight this presenter to give users visual feedback of where it will land.
                Highlighted = true;

                // Mark the event as handled so when the event bubbles up, other parent presenters will not highlight themselves as if they
                // will accept the drag-drop evaluatable instead.
                e.Handled = true;
            }

            // If the dragged data can't be accepted, don't mark the event as handled which will allow bubbling of the event to parent
            // presenters to allow them to check to see if they can handle the data.
        }

        /// <summary>
        /// Method that handles when the user is dragging an item and their mouse leaves this presenter. Remove any highlighting that is
        /// currently set.
        /// </summary>
        protected override void OnDragLeave(DragEventArgs e) {
            Highlighted = false;
        }

        /// <summary>
        /// Method that fires when the mouse moves while dragging something over this presenter. If the dragged evaluatable data can be
        /// accepted by this presenter, will indicate which effect will occur depending on whether the user is holding the CTRL key.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragOver(DragEventArgs e) {
            // If the evaluatable data can be accepted by this presenter...
            if (TryGetData(e.Data, out _, out _)) {
                // If the user is holding the CTRL key, indicate they will copy the evaluatable, otherwise they'll move it.
                e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) > 0 ? DragDropEffects.Copy : DragDropEffects.Move;
                // Mark the event as handled so any parent presenters don't attempt to override the effects with their own logic.
                e.Handled = true;
            } else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Method to handle dropping a dragged item on the presenter. If the dragged item is an evaluatable and can be accepted, it will
        /// replaced the current expression (and trigger an event). If the item cannot be accepted, it is left unhandled so as to bubble.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrop(DragEventArgs e) {
            if (TryGetData(e.Data, out var eval, out var source)) {

                // Check for a special case when the item is being dragged into an evaluatable slot of itself (e.g. dragging an 'Or' into one of it's own operands). Disallow this.
                if (source.IsParentOf(this))
                    return;

                // If the dragged item is not a parent of this, or this isn't a parent of the dragged item, simply swap them round

                // Before we update this evaluatable, keep a reference to it for the event listener
                var oldExpression = Expression;

                // Update the expression depending on source and ctrl key:
                var ctrlKey = (e.KeyStates & DragDropKeyStates.ControlKey) > 0;
                if (source != null && ctrlKey) {
                    // If ctrl key is down and we are dragging from another presenter (i.e. not a spawner), clone the existing evaluatable into this
                    Expression = eval.Clone();

                    // If the dragged eval was a child of this, the original source will have implicitly removed from the expression, so we need to dispose it
                    if (this.IsParentOf(source)) {
                        (eval as IDisposable)?.Dispose();
                        source.Expression = null;
                    }

                } else if (source != null && !ctrlKey) {
                    // If ctrl key is NOT down, and we are dragging from another presenter (not spawner), swap the evaluatable of that presenter with this one
                    // We also need to call the changed event on the other presenter (since this is not done by the DependencyProperty to prevent a invoker-subscriber loop with WPF updating itself)

                    // Also check for a special case when the item being dragged is replacing a parent. In this case we only want to update this expression. If we use the regular logic,
                    // it will set the other evaluatable to be a part of itself, which then tries to update the UI and causes a stack overflow due to recursively creating the UI.
                    source.Expression = this.IsParentOf(source) ? null : Expression;
                    source.ExpressionChanged?.Invoke(source, new ExpressionChangeEventArgs { NewExpression = Expression, OldExpression = eval });

                    // For the special parent-child drag, this expression is now no longer needed, so dispose it
                    if (this.IsParentOf(source))
                        (Expression as IDisposable)?.Dispose();

                    Expression = eval;

                } else {
                    // Otherwise if we are dragging from a spawner (regardless of whether the ctrl key is down) just set this expression to the data (the spawner
                    // is responsible for instantiating a new evaluatable for us).
                    Expression = eval;
                }

                // Raise an event to indicate the change.
                ExpressionChanged?.Invoke(this, new ExpressionChangeEventArgs { OldExpression = oldExpression, NewExpression = Expression });

                // Reset any highlighting that this presenter has due to dragging an acceptable item over this presenter.
                Highlighted = false;

                // Mark event as handled so parent presenters do not end up also handling the data.
                e.Handled = true;
            }
        }

        /// <summary>Event that is applied to the drag area that allows the user to pick up this evaluatable.</summary>
        private void DragArea_StartDrag(object sender, MouseEventArgs e, Point initialPoint) {
            var @do = new DataObject(Expression);
            @do.SetData("SourcePresenter", this);
            DragDrop.DoDragDrop(this, @do, DragDropEffects.Move | DragDropEffects.Copy);
        }

        /// <summary>Attempts to get an evaluatable from the suppliied data object. Will return true/false indicating if data is of correct format
        /// (an <see cref="Evaluatable{T}"/> where T matches the <see cref="EvalType"/> property.</summary>
        private bool TryGetData(IDataObject @do, out IEvaluatable evaluatable, out Control_EvaluatablePresenter source) => EvaluatableHelpers.TryGetData(@do, out evaluatable, out source, EvalType);
        #endregion
    }


    /// <summary>
    /// Converter that takes a type and returns the color (as defined in the theme) that is used to represent evaluatables of this type.
    /// </summary>
    public class EvaluatableBackgroundSelector : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var dic = App.Current.FindResource("OverridesTypeColors") as ResourceDictionary;
            return (value != null && dic.Contains(value) ? dic[value] : App.Current.FindResource("OverridesTypeFallbackColor")) as SolidColorBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }


    /// <summary>
    /// Event arguments passed to subscribers when the IEvaluatable expression changes on a <see cref="Control_EvaluatablePresenter"/>.
    /// </summary>
    public class ExpressionChangeEventArgs : EventArgs {
        public IEvaluatable OldExpression { get; set; }
        public IEvaluatable NewExpression { get; set; }
    }


    public static class EvaluatablePresenterAddons {
        public static bool GetShowDebugInfo(DependencyObject obj) => (bool)obj.GetValue(ShowDebugInfoProperty);
        public static void SetShowDebugInfo(DependencyObject obj, bool value) => obj.SetValue(ShowDebugInfoProperty, value);

        public static readonly DependencyProperty ShowDebugInfoProperty =
            DependencyProperty.RegisterAttached("ShowDebugInfo", typeof(bool), typeof(EvaluatablePresenterAddons), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
    }
}
