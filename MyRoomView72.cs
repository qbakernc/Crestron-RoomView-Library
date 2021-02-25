using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.Fusion;

namespace AisleRoomviewLibrary
{
    public class MyRoomView72
    {
        public delegate void VolumeMuteEventHandler(object source, MyCustomArgs args);
        public event VolumeMuteEventHandler VolumeMuteToggle;

        public delegate void VideoMuteEventHandler(object source, MyCustomArgs args);
        public event VideoMuteEventHandler VideoMuteToggle;

        public delegate void SystemPowerEventHandler(object source, MyCustomArgs args);
        public event SystemPowerEventHandler SystemPowerChange;

        public delegate void DisplayPowerEventHandler(object source, MyCustomArgs args);
        public event DisplayPowerEventHandler DisplayPowerChange;

        private RoomView72 roomView;
        private uint videoMute;
        private uint volumeMute;
        private uint configRead;
        private uint currentSource;
        private uint programVersion;
        private string programName;

        public MyRoomView72(CrestronControlSystem controlSystem, uint videoMute, uint volumeMute, uint configRead, uint currentSource, uint programVersion, string programName)
        {
            this.videoMute = videoMute;
            this.volumeMute = volumeMute;
            this.configRead = configRead;
            this.currentSource = currentSource;
            this.programVersion = programVersion;
            this.programName = programName;

            try {

                roomView = new RoomView72(0x07, controlSystem);
                roomView.FusionStateChange += new FusionStateEventHandler(roomView_FusionStateChange);
                roomView.OnlineStatusChange += new OnlineStatusChangeEventHandler(roomView_OnlineStatusChange);

                roomView.AddSig(eSigType.Bool, videoMute, "Mute Video", eSigIoMask.InputOutputSig);
                roomView.AddSig(eSigType.Bool, volumeMute, "Mute Audio", eSigIoMask.InputOutputSig);
                roomView.AddSig(eSigType.Bool, configRead, "Read Config", eSigIoMask.OutputSigOnly);
                roomView.AddSig(eSigType.String, currentSource, "Source", eSigIoMask.InputSigOnly);
                roomView.AddSig(eSigType.String, programVersion, "Program Version", eSigIoMask.InputSigOnly);

                roomView.SystemPowerOn.OutputSig.Name = "System Power On";
                roomView.SystemPowerOff.OutputSig.Name = "System Power Off";
                roomView.DisplayPowerOn.OutputSig.Name = "Display Power On";
                roomView.DisplayPowerOff.OutputSig.Name = "Display Power Off";
                roomView.UserDefinedBooleanSigDetails[videoMute].OutputSig.Name = "Mute Video";
                roomView.UserDefinedBooleanSigDetails[volumeMute].OutputSig.Name = "Mute Audio";
                roomView.UserDefinedBooleanSigDetails[configRead].OutputSig.Name = "Read Config";

                FusionRVI.GenerateFileForAllFusionDevices();
                roomView.Register();
            }
            catch (Exception e) {

                CrestronConsole.PrintLine("Roomview Registration Failed: " + e);
            }
        }

        public bool SystemPower
        {
            set { roomView.SystemPowerOn.InputSig.BoolValue = value; }
        }

        public bool DisplayPower
        {
            set { roomView.DisplayPowerOn.InputSig.BoolValue = value; }
        }

        public bool VolumeMute
        {
            set { roomView.UserDefinedBooleanSigDetails[volumeMute].InputSig.BoolValue = value; }
        }

        public bool VideoMute
        {
            set { roomView.UserDefinedBooleanSigDetails[videoMute].InputSig.BoolValue = value; }
        }

        public string CurrentSource
        {
            set { roomView.UserDefinedStringSigDetails[currentSource].InputSig.StringValue = value; }
        }

        public ushort LampHours
        {
            set { roomView.DisplayUsage.InputSig.UShortValue = value; }
        }

        void roomView_FusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            BooleanSigData sigData = (BooleanSigData)args.UserConfiguredSigDetail;

            switch (sigData.OutputSig.Name) {
                case "System Power On":
                    if (sigData.OutputSig.BoolValue) {
                        roomView.SystemPowerOn.InputSig.BoolValue = true;
                        SystemPowerChange.Invoke(this, new MyCustomArgs(true));
                        CrestronConsole.PrintLine("System Power is on");
                    }
                    break;

                case "System Power Off":
                    if (sigData.OutputSig.BoolValue) {
                        roomView.SystemPowerOn.InputSig.BoolValue = false;
                        SystemPowerChange.Invoke(this, new MyCustomArgs(false));
                        CrestronConsole.PrintLine("System Power is off");
                    }
                    break;

                case "Display Power On":
                    if (sigData.OutputSig.BoolValue) {
                        roomView.DisplayPowerOn.InputSig.BoolValue = true;
                        DisplayPowerChange.Invoke(this, new MyCustomArgs(true));
                        CrestronConsole.PrintLine("Display Power is on");
                    }
                    break;

                case "Display Power Off":
                    if (sigData.OutputSig.BoolValue) {
                        DisplayPowerChange.Invoke(this, new MyCustomArgs(false));
                        CrestronConsole.PrintLine("Display Power is off");
                    }
                    break;

                case "Mute Video":
                    if (sigData.OutputSig.BoolValue) {
                        roomView.UserDefinedBooleanSigDetails[11].InputSig.BoolValue = !roomView.UserDefinedBooleanSigDetails[11].InputSig.BoolValue;
                        VideoMuteToggle.Invoke(this, new MyCustomArgs(roomView.UserDefinedBooleanSigDetails[11].InputSig.BoolValue));
                        CrestronConsole.PrintLine("Mute Video Toggle");
                    }
                    break;

                case "Mute Audio":
                    if (sigData.OutputSig.BoolValue) {
                        roomView.UserDefinedBooleanSigDetails[12].InputSig.BoolValue = !roomView.UserDefinedBooleanSigDetails[12].InputSig.BoolValue;
                        VolumeMuteToggle.Invoke(this, new MyCustomArgs(roomView.UserDefinedBooleanSigDetails[12].InputSig.BoolValue));
                        CrestronConsole.PrintLine("Audio Mute Toggle");
                    }
                    break;

            }
        }

        void roomView_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            CrestronConsole.PrintLine(String.Format("Roomview is {0}.", currentDevice.IsOnline));
            roomView.UserDefinedStringSigDetails[programVersion].InputSig.StringValue = programName;
        }
    }

    public class MyCustomArgs : EventArgs
    {
        private bool state;

        public MyCustomArgs(bool state)
        {
            this.state = state;
        }

        public bool State
        {
            get { return state; }
        }
    }
}

