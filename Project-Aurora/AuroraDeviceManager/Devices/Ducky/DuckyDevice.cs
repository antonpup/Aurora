using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Common.Devices;
using Common.Utils;
using HidSharp;

namespace AuroraDeviceManager.Devices.Ducky
{
    class DuckyDevice : DefaultDevice
    {
        private Color processedColor;
        private (int PacketNum, int OffsetNum) currentKeyOffset;
        private Stopwatch packetDelay = new Stopwatch(); //Stopwatch used for timer resolution, System.Timer.Timer resolution is ~15ms, Stopwatch in HiRes mode is 10,000,000 per ms.
        private int progress; //A helper number for the progress of sending the packet using the Stopwatch.
        private bool updateSuccess, updating, delayReset;

        HidDevice duckyKeyboard;
        HidStream packetStream;
        byte[] colourMessage = new byte[640], prevColourMessage = new byte[640];
        byte[] colourHeader = { 0x56, 0x83, 0x00 };

        public override string DeviceName => "Ducky";

        protected override Task<bool> DoInitialize()
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
                        IsInitialized = duckyKeyboard.TryOpen(out packetStream);
                        //This line initializes the keyboard in to letting the LEDs be controlled over USB HID.
                        packetStream.Write(DuckyRGBMappings.DuckyTakeover);
                        progress = -1;
                    }
                    catch
                    {
                        IsInitialized = false;
                    }
                    break;
                }
                else
                {
                    IsInitialized = false;
                }
            }

            return Task.FromResult(IsInitialized);
        }

        protected override Task Shutdown()
        {
            if (!IsInitialized)
                return Task.CompletedTask;

            //This one is a little smaller, 81 packets. This tells the keyboard to no longer allow USB HID control of the LEDs.
            //You can tell both the takeover and release work because the keyboard will flash the same as switching to profile 1. (The same lights when you push FN + 1)
            try
            {
                packetStream.Write(DuckyRGBMappings.DuckyRelease);
            }
            catch { }
            
            packetStream?.Dispose();
            packetStream?.Close();
            progress = -2;
            IsInitialized = false;
            return Task.CompletedTask;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            foreach (KeyValuePair<DeviceKeys, Color> kc in keyColors)
            {
                //This keyboard doesn't take alpha (transparency) values, so we do this:
                processedColor = CommonColorUtils.CorrectWithAlpha(kc.Value);

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

            if (!prevColourMessage.SequenceEqual(colourMessage) && IsInitialized && progress == -1)
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
                progress = 0;
                packetDelay.Restart();
                colourMessage.CopyTo(prevColourMessage, 0);
                while (progress >= 0)
                {
                    if (packetDelay.ElapsedMilliseconds % 2 == 1)
                    {
                        delayReset = true;
                    }
                    if (packetDelay.ElapsedMilliseconds % 2 == 0 && delayReset)
                    {
                        updating = false;
                        delayReset = false;
                    }

                    if (!updating)
                    {
                        updating = true;
                        try
                        {
                            if (progress < 9)
                            {
                                packetStream.Write(colourMessage, Packet(progress), 65);
                                progress++;
                            }
                            else
                            {
                                packetDelay.Stop();
                                //This is to account for the last byte in the last packet to not overflow. The byte is 0x00 anyway so it won't matter if I leave the last byte out.
                                packetStream.Write(colourMessage, Packet(progress), 64);
                                updateSuccess = true;
                                progress = -1; //I'm using the progress int as a flag to tell UpdateDevice() when it can activate again
                            }
                        }
                        catch
                        {
                            packetDelay.Stop();
                            updateSuccess = false;
                            progress = -1;
                        }
                    }
                }
                
                return Task.FromResult(updateSuccess);
            }
            return Task.FromResult(true);
        }

        private int Packet(int packetNum) => packetNum * 64;

        private HidDevice GetDuckyKeyboard(int VID, int PID) => DeviceList.Local.GetHidDevices(VID, PID).FirstOrDefault(HidDevice => HidDevice.GetMaxInputReportLength() == 65);
    }
}
