using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Aurora
{
    /// <summary>
    /// Class for subscribing to various HID input events
    /// </summary>
    public class InputEventsSubscriptions : IDisposable
    {
        private IKeyboardMouseEvents input_hook = null;

        /// <summary>
        /// Event for a Key pressed Down on a keyboard
        /// </summary>
        public event KeyEventHandler KeyDown;

        /// <summary>
        /// Event for a Key pressed on a keyboard
        /// </summary>
        public event KeyPressEventHandler KeyPress;

        /// <summary>
        /// Event for a Key released on a keyboard
        /// </summary>
        public event KeyEventHandler KeyUp;

        /// <summary>
        /// Event for a mouse button pressed
        /// </summary>
        public event MouseEventHandler MouseClick;

        public bool Initialize()
        {
            try
            {
                input_hook = Hook.GlobalEvents();

                input_hook.KeyDown += Input_hook_KeyDown;
                input_hook.KeyPress += Input_hook_KeyPress;
                input_hook.KeyUp += Input_hook_KeyUp;
                input_hook.MouseClick += Input_hook_MouseClick;
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception during InputEventSubscriptions.Initialize(). Exception: " + exc, Logging_Level.Error);
                return false;
            }

            return true;
        }

        private void Input_hook_MouseClick(object sender, MouseEventArgs e)
        {
            Task.Factory.StartNew(
                () => { MouseClick?.Invoke(sender, e); }
            );
        }

        private void Input_hook_KeyUp(object sender, KeyEventArgs e)
        {
            Task.Factory.StartNew(
                () => { KeyUp?.Invoke(sender, e); }
            );
        }

        private void Input_hook_KeyPress(object sender, KeyPressEventArgs e)
        {
            Task.Factory.StartNew(
                () => { KeyPress?.Invoke(sender, e); }
            );
        }

        private void Input_hook_KeyDown(object sender, KeyEventArgs e)
        {
            Task.Factory.StartNew(
                () => { KeyDown?.Invoke(sender, e); }
            );

            //Handle special cases
            if ((e.KeyCode == Keys.VolumeUp || e.KeyCode == Keys.VolumeDown) && e.Modifiers == Keys.Alt)
                e.Handled = true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    input_hook.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~InputEventsSubscriptions() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
