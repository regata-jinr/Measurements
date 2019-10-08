﻿using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   StartMeasurements();
        bool   NextSample(ref IDetector d);
        void   PrevSample(ref IDetector d);
        void   MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector det);
        void   MakeSamplesCurrentOnAllDetectorsByNumber(int n);
        void   PauseMeasurements();
        void   StopMeasurements(); //Pause and Clear
        void   SaveMeasurement(ref IDetector d);
        void   SaveSpectra(ref IDetector d);
        void   SaveSession(string nameOfSession, bool isPublic = false);
        void   ContinueMeasurements();
        void   ClearMeasurements();
        void   Dispose();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);

        event Action    SessionComplete;
        event Action<MeasurementInfo>    MeasurementOfSampleDone;
        event Action CurrentSampleChanged;
        event Action<string>   MeasurementDone;

        AcquisitionModes      CountMode              { get; set; }
        SpreadOptions         SpreadOption           { get; set; }
        int                   Counts                 { get; set; }
        string                Type                   { get; set; }
        string                Name                   { get; set; }
        decimal               Height                 { get; set; }
        string                Note                   { get; set; }
        DateTime              CurrentIrradiationDate { get; set; }
        List<DateTime>       IrradiationDateList    { get; }
        List<IrradiationInfo> IrradiationList        { get; }
        List<MeasurementInfo> MeasurementList        { get; }
        List<IDetector>       ManagedDetectors       { get; }

        Dictionary<string, List<IrradiationInfo>> SpreadSamples { get; }
 
    }
}
