namespace OcrWaterMeter.Shared
{


    public class WaterMeterDebugData
    {
        public decimal Value { get; set; }

        public ImageData Image { get; set; }

        public IEnumerable<DigitalNumber> DigitalNumbers { get; set; }

        public IEnumerable<AnalogNumber> AnalogNumbers { get; set; }
    }
}