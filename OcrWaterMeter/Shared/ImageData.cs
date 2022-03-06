namespace OcrWaterMeter.Shared
{
    public class ImageData
    {
        public byte[] Image { get; set; }
        public DateTime Created { get; set; }
        public ImageType ImageType { get; set; }
    }
}