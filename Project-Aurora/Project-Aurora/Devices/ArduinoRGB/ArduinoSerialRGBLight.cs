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

using Aurora;
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
            TURN_ON = 0, // Turn ON/OFF Lights
            TURN_ON_WITH_TRANSITION = 1, // Turn ON Lights with Color Transition

            SET_COLOR = 5, // Set Light Colors

            SET_BRIGHTNESS = 10, // Set Light Brightness
            CALIBRATE_COLOR = 11, // Calibrate Color (Set MAX white color)

            ENABLE_COLOR_TRANSITION = 20, // Light Transition = ENABLDE/DISABLE
            ENABLE_GAMMA_CORRECTION = 21, // ENABLE-DISSABLE Gamma Correction
        }

        SerialPort port;

        // Connect to Serial Port
        public bool connect(string com, int baud_rate)
        {
            port = new SerialPort(com, baud_rate); // Default: COM4, Baud rate: 57600
            port.Open();
            return port.IsOpen;
        }

        // Disconnect from Serial Port
        public void disconnect()
        {
            port.Close();
            port.Dispose();
        }

        // Turn ON
        public void turnOn()
        {
            sendMessage(formatMessage(255, 255, 255, Mode.TURN_ON));
        }

        // Turn ON (With Color Transition)
        public void turnOn(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.TURN_ON_WITH_TRANSITION));
        }

        // Turn OFF
        public void turnOff()
        {
            sendMessage(formatMessage(0, 0, 0, Mode.TURN_ON));
        }

        // Set Light Color
        public void setColor(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.SET_COLOR));
        }

        // Set Light Color
        public void calibrateColor(byte color_r, byte color_g, byte color_b)
        {
            sendMessage(formatMessage(color_r, color_g, color_b, Mode.CALIBRATE_COLOR));
        }

        // Set Brightness
        public void setBrightness(byte brightness)
        {
            sendMessage(formatMessage(0, 0, brightness, Mode.SET_BRIGHTNESS));
        }

        // Enable Color Transitions
        public void enableColorTransitions(bool enable)
        {
            if(enable)
                sendMessage(formatMessage(255, 255, 255, Mode.ENABLE_COLOR_TRANSITION));
            else
                sendMessage(formatMessage(  0,   0,   0, Mode.ENABLE_COLOR_TRANSITION));
        }

        // Enable Gamma Correction
        public void enableGamaCorrection(bool enable)
        {
            if (enable)
                sendMessage(formatMessage(255, 255, 255, Mode.ENABLE_GAMMA_CORRECTION));
            else
                sendMessage(formatMessage(  0,   0,   0, Mode.ENABLE_GAMMA_CORRECTION));
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
