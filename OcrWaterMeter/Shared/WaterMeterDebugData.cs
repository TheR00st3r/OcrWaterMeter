namespace OcrWaterMeter.Shared
{


    public class WaterMeterDebugData
    {
        public decimal Value { get; set; }

        public decimal LastValue { get; set; }

        public IEnumerable<DigitalNumber> DigitalNumbers { get; set; } = Enumerable.Empty<DigitalNumber>();

        public IEnumerable<AnalogNumber> AnalogNumbers { get; set; } = Enumerable.Empty<AnalogNumber>();
        public DateTime LastValueDate { get; set; }
        public DateTime ValueDate { get; set; }
    }
}