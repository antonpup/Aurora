using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HidSharp;
using Aurora.Utils;
using System.Threading;

namespace Aurora.Devices.Ducky
{
    class DuckyDevice : Device
    {
        private static string deviceName = "Ducky";
        private bool isInitialized = false;
        private long lastUpdateTime = 0;
        private readonly Stopwatch watch = new Stopwatch();
        private Color processedColor;
        private (int PacketNum, int OffsetNum) currentKeyOffset;

        HidDevice duckyKeyboard;
        HidStream packetStream;
        byte[] colourMessage = new byte[640], prevColourMessage = new byte[640];
        byte[] colourHeader = { 0x56, 0x83, 0x00 };

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public string GetDeviceName()
        {
            return deviceName;
        }

        public string GetDeviceDetails()
        {
            return deviceName + ": " + (isInitialized ? "Connected" : "Not initialized");
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public bool Initialize()
        {
            //Sets the initialize colour change packet
            DuckyRGBMappings.DuckyStartingPacket.CopyTo(colourMessage, Packet(0) + 1);
            //Headers for each colour packet
            for (byte i = 0; i < 8; i++)
            {
                colourHeader[2] = i;
                colourHeader.CopyTo(colourMessage, Packet(i + 1) + 1);
            }
            //First colour packet has extra data
            DuckyRGBMappings.DuckyInitColourBytes.CopyTo(colourMessage, Packet(1) + 5);
            //Sets terminate colour packet
            DuckyRGBMappings.DuckyTerminateColourBytes.CopyTo(colourMessage, Packet(9) + 1);

            foreach (int keyboardID in DuckyRGBMappings.KeyboardIDs)
            {
                duckyKeyboard = GetDuckyKeyboard(DuckyRGBMappings.DuckyID, keyboardID);
                if (duckyKeyboard != null)
                {
                    try
                    {
                        isInitialized = duckyKeyboard.TryOpen(out packetStream);
                        //This uses a monstrous 501 packets to initialize the keyboard in to letting the LEDs be controlled over USB HID.
                        foreach (byte[] controlPacket in DuckyRGBMappings.DuckyTakeover)
                        {
                            packetStream.Write(controlPacket);
                        }
                    }
                    catch
                    {
                        isInitialized = false;
                    }
                    break;
                }
                else
                {
                    isInitialized = false;
                }
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (!isInitialized)
                return;

            //This one is a little smaller, 81 packets. This tells the keyboard to no longer allow USB HID control of the LEDs.
            //You can tell both the takeover and release work because the keyboard will flash the same as switching to profile 1. (The same lights when you push FN + 1)
            foreach (byte[] controlPacket in DuckyRGBMappings.DuckyRelease)
            {
                try
                {
                    packetStream.Write(controlPacket);
                }
                catch
                {
                    break;
                }
            }
            
            packetStream?.Dispose();
            packetStream?.Close();
            isInitialized = false;
        }

        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            foreach (KeyValuePair<DeviceKeys, Color> kc in keyColors)
            {
                //This keyboard doesn't take alpha (transparency) values, so we do this:
                processedColor = ColorUtils.CorrectWithAlpha(kc.Value);

                //This if statement grabs the packet offset from the key that Aurora wants to set, using DuckyColourOffsetMap.
                //It also checks whether the key exists in the Dictionary, and if not, doesn't try and set the key colour.
                if(!DuckyRGBMappings.DuckyColourOffsetMap.TryGetValue(kc.Key, out currentKeyOffset)){
                    continue;
                }

                //The colours are encoded using RGB bytes consecutively throughout the 10 packets, which are offset with DuckyColourOffsetMap.
                colourMessage[Packet(currentKeyOffset.PacketNum) + currentKeyOffset.OffsetNum + 1] = processedColor.R;
                //To account for the headers in the next packet, the offset is pushed a further four bytes (only required if the R byte starts on the last byte of a packet).
                if (currentKeyOffset.OffsetNum == 63)
                {
                    colourMessage[Packet(currentKeyOffset.PacketNum) + currentKeyOffset.OffsetNum + 6] = processedColor.G;
                    colourMessage[Packet(currentKeyOffset.PacketNum) + currentKeyOffset.OffsetNum + 7] = processedColor.B;
                }
                else
                {
                    colourMessage[Packet(currentKeyOffset.PacketNum) + currentKeyOffset.OffsetNum + 2] = processedColor.G;
                    colourMessage[Packet(currentKeyOffset.PacketNum) + currentKeyOffset.OffsetNum + 3] = processedColor.B;
                }
            }

            if (!prevColourMessage.SequenceEqual(colourMessage) && IsInitialized())
            {
                //Everything previous to setting the colours actually just write the colour data to the ColourMessage byte array.
                /*
                 The keyboard is only set up to change all key colours at once, using 10 USB HID packets. They consist of:
                 One initializing packet
                 Eight colour packets (although the eighth one isn't used at all)
                 and one terminate packet
             
                 These packets are 64 bytes each (technically 65 but the first byte is just padding, which is why there's the .Take(65) there)
                 Each key has its own three bytes for r,g,b somewhere in the 8 colour packets. These positions are defined in the DuckyColourOffsetMap
                 The colour packets also have a header. (You might be able to send these packets out of order, and the headers will tell the keyboard where it should be, but IDK)*/
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        if (i < 9)
                        {
                            packetStream.Write(colourMessage, Packet(i), 65);
                        }
                        else
                        {
                            //This is to account for the last byte in the last packet to not overflow. The byte is 0x00 anyway so it won't matter if I leave the last byte out.
                            packetStream.Write(colourMessage, Packet(i), 64);
                        }
                    }
                    catch
                    {
                        Reset();
                        return false;
                    }
                    Thread.Sleep(2);
                }
                colourMessage.CopyTo(prevColourMessage, 0);
                return true;
            }
            return true;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private int Packet(int packetNum) => packetNum * 64;

        private HidDevice GetDuckyKeyboard(int VID, int PID) => DeviceList.Local.GetHidDevices(VID, PID).FirstOrDefault(HidDevice => HidDevice.GetMaxInputReportLength() == 65);
    }
}
