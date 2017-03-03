/*
 * MIT License
 * 
 * Copyright (c) 2017 Robert Koszewski
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobertKoszewski.DeviceDrivers
{
    class ArduinoSerialRGBLight
    {

        public enum Mode : byte
        {
            TURNOFF = 0, // Turn off Lights
            TURNON = 1, // Turn on Lights
            TURNONTRANSITION = 2, // Turn on Lights with Color Transition
            SETCOLOR = 2, // Set Light Colors

            SETBRIGHTNESS = 10, // Set Light Brightness
            CALIBRATECOLOR = 11, // Calibrate Color (Set MAX white color)

            TRANSITION_ENABLE = 20, // Light Transition = Enable
            TRANSITION_DISABLE = 21, // Light Transition = Disable
        }

        SerialPort port;

        // Connect to Serial Port
        public void connect(string com, int baud_rate)
        {
            // port = new SerialPort("COM4", 57600);
            port = new SerialPort(com, baud_rate);
            port.Open();
        }

        // Disconnect from Serial Port
        public void disconnect()
        {
            port.Close();
        }

        // Turn ON
        public void turnOn()
        {
            sendMessage(formatMessage(0, 0, 0, Mode.TURNON));
        }

        // Turn ON
        public void turnOn(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.TURNONTRANSITION));
        }

        // Turn OFF
        public void turnOff()
        {
            sendMessage(formatMessage(0, 0, 0, Mode.TURNOFF));
        }

        // Set Light Color
        public void setColor(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.SETCOLOR));
        }

        // Set Light Color
        public void calibrateColor(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.CALIBRATECOLOR));
        }

        // Set Brightness
        public void setBrightness(byte brightness)
        {
            sendMessage(formatMessage(0, 0, brightness, Mode.SETBRIGHTNESS));
        }

        // Send Message to Device
        public void sendMessage(UInt32 message)
        {
            try
            {
                port.WriteLine(message + "");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
        }

        // Wrap Message to be sent to Serial
        /*
        private static byte[] wrapMessage(UInt32 message)
        {
            return new byte[] {
                 1, // Starts with 1111 1111
                ((byte) ((message >> 24) & 0xff)),
                ((byte) ((message >> 16) & 0xff)),
                ((byte) ((message >> 8) & 0xff)),
                ((byte) (message & 0xff)),
                 0 // Ends with 0000 0000
            };
        }
        */

        // Format Message
        private static UInt32 formatMessage(byte a, byte b, byte c, Mode mode)
        {
            return (UInt32)(((((((int)mode << 8) | a) << 8) | b) << 8) | c);
        }
    }
}
