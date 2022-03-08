namespace OcrWaterMeter.Shared
{
    public class ImageData
    {

        public ImageData(byte[] image, DateTime created, ImageType imageType) : this(image, created, imageType, 0)
        {

        }

        public ImageData(byte[] image, DateTime created, ImageType imageType, int number)
        {
            Image = image;
            Created = created;
            ImageType = imageType;
            Number = number;
        }

        public byte[] Image { get; }

        public DateTime Created { get; }

        public ImageType ImageType { get; }

        public int Number { get; }
    }
}