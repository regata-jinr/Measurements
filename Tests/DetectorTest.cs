﻿using Xunit;
using CanberraDataAccessLib;
using System;
using Xunit.Abstractions;
using AutoMapper;

namespace Measurements.Core.Tests
{
   //TODO: add stress-test!
   //TODO: add correct sequence for tests
    public class Detectors
    {
        public Detector d1;
        //public Detector d5;
        //public Detector d6;
        //public Detector d7;

        public Detectors()
        {
            d1 = new Detector("D1");
            //d5 = new Detector("D5");
            //d6 = new Detector("D6");
            //d7 = new Detector("D7");
        }
    }

    public class DetectorsTest : IClassFixture<Detectors>
    {
        private readonly ITestOutputHelper output;
        public Detectors _detectors;
        DataAccess f1;

        public DetectorsTest(Detectors dets, ITestOutputHelper output)
        {
            _detectors = dets;
            f1 = new DataAccess();
            System.Threading.Thread.Sleep(1000);
            this.output = output;
        }

        [Fact]
        public void Logs()
        {
            Assert.True(System.IO.File.Exists($"{System.IO.Directory.GetCurrentDirectory()}\\MeasurementsLogs\\{DateTime.Now.ToString("yyyy-MM-dd")}.log"));
        }

        [Fact]
        public void Names()
        {
            Assert.Equal("D1", _detectors.d1.Name);
        }

        [Fact]
        public void Statuses()
        {
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
        }

        [Fact]
        public void Connections()
        {
            Assert.True(_detectors.d1.IsConnected);
        }

        [Fact]
        public void StartStopStartStopSave()
        {
            var sd = new IrradiationInfo()
            {
                CountryCode = "RO",
                ClientNumber = "2",
                Year = "19",
                SetNumber = "12",
                SetIndex = "b",
                SampleNumber = "2",
                Weight = 0.2m,
                Assistant = "bdrum",
                Note = "test2",
                DateTimeStart = DateTime.Now,
                DateTimeFinish = DateTime.Now.AddSeconds(3),
                Duration = 3
            };

            var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MeasurementsCore"));
            var mapper = new Mapper(configuration);
            var m = mapper.Map<MeasurementInfo>(sd);
            m.Duration = 5;
            m.Detector = "D1";
            m.Height = 10;
            m.Type = "SLI";
            m.FileSpectra = "testD1";
            m.Assistant = "bdrum";
            m.Note = "bdrum-test";

            _detectors.d1.FillSampleInformation(m, sd);

            _detectors.d1.Start();

            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            Assert.NotEqual(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)), 2);
            _detectors.d1.Stop();

            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            Assert.Equal(2, (int)Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)));
            _detectors.d1.Start();
            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            Assert.NotEqual(2, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)), 2);
            _detectors.d1.Stop();
            _detectors.d1.Save();

            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);

            System.Threading.Thread.Sleep(1000);
            Assert.True(System.IO.File.Exists(_detectors.d1.FullFileSpectraName));

            f1.Open(_detectors.d1.FullFileSpectraName);

            Assert.Equal($"{_detectors.d1.CurrentMeasurement.SampleKey}", f1.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
            Assert.Equal(_detectors.d1.CurrentMeasurement.Assistant, f1.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name
            Assert.Equal(_detectors.d1.CurrentMeasurement.Note, f1.Param[ParamCodes.CAM_T_SDESC1].ToString());
            Assert.Equal(_detectors.d1.CurrentMeasurement.SetKey, f1.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
            Assert.Equal(_detectors.d1.RelatedIrradiation.Weight.ToString(), f1.Param[ParamCodes.CAM_F_SQUANT].ToString()); // weight
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
            Assert.Equal("gram", f1.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
            Assert.Equal(_detectors.d1.RelatedIrradiation.DateTimeStart.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
            Assert.Equal(_detectors.d1.RelatedIrradiation.DateTimeFinish.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
            Assert.Equal(_detectors.d1.CurrentMeasurement.Type, f1.Param[ParamCodes.CAM_T_STYPE].ToString());
            Assert.Equal(_detectors.d1.CurrentMeasurement.Height.Value, decimal.Parse(f1.Param[ParamCodes.CAM_T_SGEOMTRY].ToString()),2);

            f1.Close();

            System.IO.File.Delete(_detectors.d1.FullFileSpectraName);
            Assert.False(System.IO.File.Exists(_detectors.d1.FullFileSpectraName));
        }

        [Fact]
        public void Disconnections()
        {
            System.Threading.Thread.Sleep(2000);

            Assert.True(_detectors.d1.IsConnected);
            _detectors.d1.Disconnect();

            System.Threading.Thread.Sleep(2000);

            Assert.False(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.off, _detectors.d1.Status);

            _detectors.d1.Connect();

        }
    }

}

