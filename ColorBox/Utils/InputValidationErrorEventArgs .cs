/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorBox
{
    internal delegate void InputValidationErrorEventHandler(object sender, InputValidationErrorEventArgs e);

    internal class InputValidationErrorEventArgs : EventArgs
    {
        Exception _exception;
        bool _throwException;

        public InputValidationErrorEventArgs(Exception e)
        {
            Exception = e;
        }
    
 
        public Exception Exception
        {
            get
            {
                return _exception;
            }
            private set
            {
                _exception = value;
            }
        }
            
        public bool ThrowException
        {
            get
            {
                return _throwException;
            }
            set
            {
                _throwException = value;
            }
        }        
    }

    internal interface IValidateInput
    {
        event InputValidationErrorEventHandler InputValidationError;
        bool CommitInput();
    }
}