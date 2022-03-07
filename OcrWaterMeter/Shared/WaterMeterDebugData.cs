namespace OcrWaterMeter.Shared
{


    public class WaterMeterDebugData
    {
        public double Value { get; set; }

        public ImageData Image { get; set; }

        public IEnumerable<DigitalNumber> DigitalNumbers { get; set; }
    }
}