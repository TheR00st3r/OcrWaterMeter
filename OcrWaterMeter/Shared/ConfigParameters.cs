namespace OcrWaterMeter.Shared
{
    public class ConfigParameters
    {
        public const string ImageSrc = nameof(ImageSrc);
        public const string ImageAngle = nameof(ImageAngle);
        public const string InitialValue = nameof(InitialValue);
        public const string CropOffsetHorizontal = nameof(CropOffsetHorizontal);
        public const string CropOffsetVertical= nameof(CropOffsetVertical);
        public const string CropWidth = nameof(CropWidth);
        public const string CropHeight = nameof(CropHeight);
        public const string Lightness = nameof(Lightness);
        public const string Contrast = nameof(Contrast);

        public const string AnalogColorR = nameof(AnalogColorR);
        public const string AnalogColorG = nameof(AnalogColorG);
        public const string AnalogColorB = nameof(AnalogColorB);

        public const string LastMeasurement = nameof(LastMeasurement);
        public const string MaxWaterPerHour = nameof(MaxWaterPerHour);
    }
}