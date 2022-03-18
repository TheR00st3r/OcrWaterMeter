using LiteDB;
using Microsoft.AspNetCore.Mvc;
using OcrWaterMeter.Server.Database;
using OcrWaterMeter.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using Tesseract;

namespace OcrWaterMeter.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WaterMeterController : ControllerBase
    {
        private readonly ILogger<WaterMeterController> _Logger;
        private readonly LiteDbContext _DbContext;

        public WaterMeterController(ILogger<WaterMeterController> logger, LiteDbContext dbContext)
        {
            _Logger = logger;
            _DbContext = dbContext;
        }

        [HttpGet("Value")]
        public async Task<IActionResult> GetValue()
        {
            var data = await LoadData();
            return Ok(data.Value);
        }

        [HttpGet("Image/{type}")]
        public ActionResult GetImage(string type)
        {
            if (Enum.TryParse<ImageType>(type, out var imageType))
            {
                var imageCollection = _DbContext.Context.GetCollection<ImageData>();

                var imageData = imageCollection.FindOne(x => x.ImageType == imageType);
                if (imageData != null)
                {
                    return base.File(imageData.Image, "image/jpeg");
                }
            }

            return NotFound();
        }


        [HttpGet("Image/{type}/{number}/")]
        public ActionResult GetImage(string type, int number)
        {
            if (Enum.TryParse<ImageType>(type, out var imageType))
            {
                var imageCollection = _DbContext.Context.GetCollection<ImageData>();
                if (imageType == ImageType.Number)
                {
                    var t = imageCollection.Find(x => x.ImageType == ImageType.Number).ToList();
                    var numberImageData = imageCollection.FindOne(x => x.ImageType == imageType && x.Number == number);
                    if (numberImageData?.Image != null)
                    {
                        return base.File(numberImageData.Image, "image/jpeg");
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("DebugData")]
        public async Task<ActionResult<WaterMeterDebugData>> GetDebugData()
        {
            return Ok(await LoadData());
        }

        [HttpGet("ConfigValues")]
        public ActionResult<IEnumerable<ConfigValue>> GetConfigValues()
        {
            var configCollection = _DbContext.Context.GetCollection<ConfigValue>();

            return Ok(configCollection.FindAll().ToList());
        }

        [HttpPost("ConfigValue")]
        public IActionResult PostConfigValue([FromBody] ConfigValue value)
        {
            var configCollection = _DbContext.Context.GetCollection<ConfigValue>();
            configCollection.DeleteMany(x => x.Key == value.Key);
            configCollection.Insert(value);

            if (value.Key.Equals(ConfigParameters.InitialValue))
            {
                if (decimal.TryParse(value.Value, out var initialValue))
                {
                    var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
                    var analogNumberCollection = _DbContext.Context.GetCollection<AnalogNumber>();
                    var allNumbers = analogNumberCollection.FindAll().OfType<NumberBase>().Concat(digitalNumberCollection.FindAll()).OrderByDescending(x => x.Factor);
                    foreach (var number in allNumbers)
                    {
                        number.Value = (int)Math.Floor(initialValue / number.Factor);
                        number.LastValue = number.Value;
                        initialValue -= number.Value * number.Factor;
                        SaveNumber(number, digitalNumberCollection, analogNumberCollection);
                    }
                }
            }

            return Ok();
        }

        [HttpPost("DigitalNumber")]
        public IActionResult PostDigitalNumber([FromBody] DigitalNumber value)
        {
            var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
            digitalNumberCollection.DeleteMany(x => x.Id == value.Id);
            digitalNumberCollection.Insert(value);
            return Ok();
        }

        [HttpPost("AnalogNumber")]
        public IActionResult PostAnalogNumber([FromBody] AnalogNumber value)
        {
            var analogNumberCollection = _DbContext.Context.GetCollection<AnalogNumber>();
            analogNumberCollection.DeleteMany(x => x.Id == value.Id);
            analogNumberCollection.Insert(value);
            return Ok();
        }

        [HttpDelete("Number/{id}")]
        public IActionResult PostDigitalNumber(int id)
        {
            var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
            digitalNumberCollection.DeleteMany(x => x.Id == id);
            var analogNumberCollection = _DbContext.Context.GetCollection<AnalogNumber>();
            analogNumberCollection.DeleteMany(x => x.Id == id);

            return Ok();
        }

        private async Task<WaterMeterDebugData> LoadData()
        {
            var result = new WaterMeterDebugData() { Value = 1 };
            try
            {
                var configCollection = _DbContext.Context.GetCollection<ConfigValue>();
                var imageCollection = _DbContext.Context.GetCollection<ImageData>();
                var image = imageCollection.FindOne(x => x.ImageType == ImageType.SrcImage);


                if (image == null || image.Created < DateTime.Now.AddMinutes(-1))
                {
                    var imageSrc = configCollection.FindOne(x => x.Key == ConfigParameters.ImageSrc);
                    using var httpClient = new HttpClient();
                    var imageData = await httpClient.GetByteArrayAsync(imageSrc.Value);

                    if (image != null)
                    {
                        imageCollection.DeleteMany(i => i.ImageType == ImageType.SrcImage);
                    }
                    
                    image = new ImageData(imageData, DateTime.Now, ImageType.SrcImage);
                    imageCollection.Insert(image);

                }

                var rotate = float.Parse(configCollection.FindOne(x => x.Key == ConfigParameters.ImageAngle)?.Value ?? "0", CultureInfo.InvariantCulture);
                var offsetHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParameters.CropOffsetHorizontal)?.Value ?? "0", CultureInfo.InvariantCulture);
                var offsetVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParameters.CropOffsetVertical)?.Value ?? "0", CultureInfo.InvariantCulture);
                var sizeHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParameters.CropWidth)?.Value ?? "0", CultureInfo.InvariantCulture);
                var sizeVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParameters.CropHeight)?.Value ?? "0", CultureInfo.InvariantCulture);

                using var imageToClone = Image.Load<Rgba32>(image.Image);
                using var rotateCopy = imageToClone.Clone(i => i.Rotate(RotateMode.Rotate180).Rotate(rotate));


                sizeHorizontal = sizeHorizontal <= 0 ? rotateCopy.Width - offsetHorizontal : sizeHorizontal;
                sizeVertical = sizeVertical <= 0 ? rotateCopy.Height - offsetVertical : sizeVertical;

                using var cropCopy = rotateCopy.Clone(i => i.Crop(new Rectangle(offsetHorizontal, offsetVertical, sizeHorizontal, sizeVertical)));

                using var ms = new MemoryStream();
                cropCopy.Save(ms, new JpegEncoder());

                if (image != null)
                {
                    imageCollection.DeleteMany(i => i.ImageType == ImageType.ProcessedImage);
                }
                image = new ImageData(ms.ToArray(), DateTime.Now, ImageType.ProcessedImage);
                imageCollection.Insert(image);

                using var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default);

                var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
                imageCollection.DeleteMany(x => x.ImageType == ImageType.Number);
                foreach (var digitalNumber in digitalNumberCollection.FindAll())
                {
                    try
                    {
                        using var digitalNumberCopy = cropCopy.Clone(i => i.Crop(new Rectangle(digitalNumber.HorizontalOffset, digitalNumber.VerticalOffset, digitalNumber.Width, digitalNumber.Height)));
                        using var digitalNumberStream = new MemoryStream();
                        digitalNumberCopy.Save(digitalNumberStream, new JpegEncoder());

                        var numberImage = new ImageData(digitalNumberStream.ToArray(), DateTime.Now, ImageType.Number, digitalNumber.Id);
                        imageCollection.Insert(numberImage);

                        using (var img = Pix.LoadFromMemory(numberImage.Image))
                        {
                            using var page = engine.Process(img, PageSegMode.SingleChar);
                            var content = page.GetText();
                            Console.WriteLine(result);
                            if (int.TryParse(content, out var numericValue))
                            {
                                digitalNumber.OcrValue = numericValue;
                            }
                        }
                        digitalNumberCollection.Update(digitalNumber);
                    }
                    catch (Exception e)
                    {
                        _Logger.LogError(e, "Error on DigitalNumber {Id}", digitalNumber?.Id);
                    }
                }

                var analogNumberCollection = _DbContext.Context.GetCollection<AnalogNumber>();
                foreach (var analogNumber in analogNumberCollection.FindAll())
                {
                    try
                    {
                        using var digitalNumberCopy = cropCopy.Clone(i => i.Crop(new Rectangle(analogNumber.HorizontalOffset, analogNumber.VerticalOffset, analogNumber.Width, analogNumber.Height)));
                        using var digitalNumberStream = new MemoryStream();
                        digitalNumberCopy.Save(digitalNumberStream, new JpegEncoder());

                        var numberImage = new ImageData(digitalNumberStream.ToArray(), DateTime.Now, ImageType.Number, analogNumber.Id);
                        imageCollection.Insert(numberImage);

                        var centerX = digitalNumberCopy.Width / 2;
                        var centerY = digitalNumberCopy.Height / 2;


                        var farestX = centerX;
                        var farestY = centerY;

                        var farestDistance = 0d;
                        digitalNumberCopy.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < accessor.Height; y++)
                            {
                                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                                foreach (ref Rgba32 pixel in pixelRow)
                                {
                                    if (pixel.R > 150 && pixel.G < 50 && pixel.B < 50)
                                    {
                                        var horizontalIndex = pixelRow.IndexOf(pixel);
                                        var verticalIndex = y;

                                        var distance = Distance(horizontalIndex, verticalIndex, centerX, centerY);
                                        if (distance > farestDistance)
                                        {
                                            farestDistance = distance;
                                            farestX = horizontalIndex;
                                            farestY = verticalIndex;
                                        }
                                    }
                                }
                            }
                        });


                        var dx = centerX - farestX;
                        var dy = centerY - farestY;
                        double angle = Math.Atan2(dy, dx) * 180 / Math.PI;

                        // Rotate 90° => 0 is on top
                        angle -= 90;

                        if (angle < 0)
                        {
                            // Fix Negative Angles
                            angle += 360;
                        }

                        var number = 10 / (360 / angle);
                        var value = (int)Math.Floor(number);
                        analogNumber.OcrValue = value;
                        analogNumberCollection.Update(analogNumber);
                    }
                    catch (Exception e)
                    {
                        _Logger.LogError(e, "Error on AnalogNumber {Id}", analogNumber?.Id);
                    }
                }

                var allNumbers = analogNumberCollection.FindAll().OfType<NumberBase>().Concat(digitalNumberCollection.FindAll()).OrderBy(x => x.Factor).ToList();
                var lastLastValue = allNumbers.Sum(x => x.LastValue * x.Factor);

                for (int i = 0; i < allNumbers.Count; i++)
                {
                    var currentNumber = allNumbers.ElementAt(i);

                    if (i == 0)
                    {
                        currentNumber.LastValue = currentNumber.Value;
                        currentNumber.Value = currentNumber.OcrValue;
                        continue;
                    }

                    var lastNumber = allNumbers.ElementAt(i - 1);
                    if ((currentNumber.OcrValue == currentNumber.Value + 1 && lastNumber.OcrValue < 8)
                        || currentNumber.OcrValue > currentNumber.Value + 1
                        || (currentNumber.Factor < 1 /* allow Jumps for small Numbers*/))
                    {
                        currentNumber.LastValue = currentNumber.Value;
                        currentNumber.Value = currentNumber.OcrValue;
                    }

                    //currentNumber.LastValue = currentNumber.Value;
                    //currentNumber.Value = currentNumber.OcrValue;
                }

                result.DigitalNumbers = digitalNumberCollection.FindAll();
                result.AnalogNumbers = analogNumberCollection.FindAll();
                result.LastValue = allNumbers.Sum(x => x.LastValue * x.Factor);
                result.Value = allNumbers.Sum(x => x.Value * x.Factor);

                var lastMeaserment = configCollection.FindOne(x => x.Key.Equals(ConfigParameters.LastMeasurement));
                var maxWaterPerHour = configCollection.FindOne(x => x.Key.Equals(ConfigParameters.MaxWaterPerHour));

                result.LastValueDate = string.IsNullOrEmpty(lastMeaserment?.Value) ? default : DateTime.Parse(lastMeaserment.Value);
                result.ValueDate = DateTime.Now;

                if (IsValidValue(result, result.LastValueDate, result.ValueDate, maxWaterPerHour))
                {
                    foreach (var currentNumber in allNumbers)
                    {
                        SaveNumber(currentNumber, digitalNumberCollection, analogNumberCollection);
                    }

                    PostConfigValue(new ConfigValue(ConfigParameters.LastMeasurement, result.ValueDate.ToString()));
                }
                else
                {
                    // Value is not possible => return last value
                    result.Value = result.LastValue;
                    result.LastValue = lastLastValue;
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Error on Loading DebugData");

                // In case of Exception try return last value;
                var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
                var analogNumberCollection = _DbContext.Context.GetCollection<AnalogNumber>();
                result.DigitalNumbers = digitalNumberCollection.FindAll();
                result.AnalogNumbers = analogNumberCollection.FindAll();
                result.Value = result.DigitalNumbers.Sum(x => x.Value * x.Factor) + result.AnalogNumbers.Sum(x => x.Value * x.Factor);
            }

            return result;
        }

        private static bool IsValidValue(WaterMeterDebugData result, DateTime lastMeasurement, DateTime currentMeasurement, ConfigValue maxWaterPerHour)
        {
            if (result.Value < result.LastValue)
            {
                return false;
            }

            if (lastMeasurement != default)
            {
                var difference = result.Value - result.LastValue;

                if (difference > 0)
                {
                    var hours = new decimal((currentMeasurement - lastMeasurement).TotalHours);
                    var differencePerHour = difference / hours;
                    var maxCmPerHour = maxWaterPerHour != null ? decimal.Parse(maxWaterPerHour.Value) : 4;
                    if (differencePerHour > maxCmPerHour)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private static void SaveNumber(NumberBase number, ILiteCollection<DigitalNumber> digitalNumberCollection, ILiteCollection<AnalogNumber> analogNumberCollection)
        {
            if (number is DigitalNumber digitalNumber)
            {
                digitalNumberCollection.Update(digitalNumber);
            }
            else if (number is AnalogNumber analogNumber)
            {
                analogNumberCollection.Update(analogNumber);
            }
        }
    }
}