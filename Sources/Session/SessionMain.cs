﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    //TODO: add docs
    //TODO: add tests

    partial class Session : ISession, IDisposable
    {
        public string Name { get; private set; }
        public string Type { get; set; }
        public decimal Height { get; set; }
        public event EventHandler SessionComplete;
        public event EventHandler MeasurementDone;
        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; set; }
        public int Counts { get; set; }
        private IrradiationInfoContext _irradiationInfoContext;
        private MeasurementInfoContext _measurementInfoContext;
        
        public IrradiationInfo CurrentSample { get; private set; }
        public MeasurementInfo CurrentMeasurement { get; private set; } 
        public List<IrradiationInfo> IrradiationList { get; private set; }
        public List<MeasurementInfo> MeasurementList { get; private set; }
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        private List<Detector> _managedDetectors;
        private bool _isDisposed = false;
        private Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes> _countModeDict;
             
        public Session()
        {
            Name = "Untitled session";
            _irradiationInfoContext = new IrradiationInfoContext();
            _measurementInfoContext = new MeasurementInfoContext();
            IrradiationList = new List<IrradiationInfo>();
            MeasurementList = new List<MeasurementInfo>();
            CurrentMeasurement = new MeasurementInfo();
            CurrentSample = new IrradiationInfo();
            _managedDetectors = new List<Detector>();
            SpreadedSamples = new Dictionary<string, List<IrradiationInfo>>();
            _countModeDict = new Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes>
                                 {
                                          { "aCountToLiveTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime },
                                          { "aCountToRealTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime },
                                          { "aCountNormal", CanberraDeviceAccessLib.AcquisitionModes.aCountNormal }
                                 };
        }

        public Session(SessionInfo session) : this()
        {
            Name = session.Name;
            Type = session.Type;
            Counts = session.Duration;
            Height = session.Height;
            CountMode = _countModeDict[session.CountMode];

            _managedDetectors.AddRange(SessionControllerSingleton.AvailableDetectors.Where(d => session.DetectorsNames.Split(',').Contains(d.Name)).ToList());

        }

        public void SaveSession(string nameOfSession, bool isBasic=false, string note = "")
        {
            Name = nameOfSession;
            var sessionContext = new SessionInfoContext();

            string assistant = null;
            if (!isBasic) assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;

            sessionContext.Add(new SessionInfo
                                              {
                                                   CountMode      = this.CountMode.ToString(),
                                                   Duration       = this.Counts,
                                                   Height         = this.Height,
                                                   Name           = this.Name,
                                                   Type           = this.Type,
                                                   Assistant      = assistant,
                                                   Note           = note,
                                                   DetectorsNames = string.Join(",", _managedDetectors.Select(n => n.Name).ToArray())
                                               }
            );
        }

        ~Session()
        {
            CleanUp(false);
        }

        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool isDisposing)
        {

            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    SessionControllerSingleton.ManagedSessions.Remove(this);
                    foreach (var d in _managedDetectors)
                        d.Dispose();
                }

                //FIXME: actually dispose already do the disconnect.
                foreach (var d in _managedDetectors)
                    d.Disconnect();

                _managedDetectors.Clear();
            }
            _isDisposed = true;
        }
    }
}