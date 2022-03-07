using LiteDB;
using Microsoft.AspNetCore.Mvc;
using OcrWaterMeter.Server.Database;
using OcrWaterMeter.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await LoadData();
            return Ok(data.Value);
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

                    return base.File(numberImageData.Image, "image/jpeg");
                }
            }

            return NotFound();
        }

        [HttpGet("Image/{type}")]
        public ActionResult GetImage(string type)
        {
            if (Enum.TryParse<ImageType>(type, out var imageType))
            {
                var imageCollection = _DbContext.Context.GetCollection<ImageData>();
                //if (imageType == ImageType.ProcessedImage)
                //{
                //    var imageDataToClone = imageCollection.FindOne(x => x.ImageType == ImageType.SrcImage);
                //    if (imageDataToClone != null)
                //    {
                //        var configCollection = _DbContext.Context.GetCollection<ConfigValue>();
                //        var rotate = float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.ImageAngle)?.Value ?? "0");
                //        var offsetHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropOffsetHorizontal)?.Value ?? "0");
                //        var offsetVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropOffsetVertical)?.Value ?? "0");
                //        var sizeHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropWidth)?.Value ?? "0");
                //        var sizeVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropHeight)?.Value ?? "0");

                //        using var image = Image.Load(imageDataToClone.Image);
                //        using var rotateCopy = image.Clone(i => i.Rotate(RotateMode.Rotate180).Rotate(rotate));


                //        //TODO Size after Rotate?
                //        sizeHorizontal = sizeHorizontal <= 0 ? rotateCopy.Width - offsetHorizontal : sizeHorizontal;
                //        sizeVertical = sizeVertical <= 0 ? rotateCopy.Height - offsetVertical : sizeVertical;

                //        using var cropCopy = rotateCopy.Clone(i => i.Crop(new Rectangle(offsetHorizontal, offsetVertical, sizeHorizontal, sizeVertical)));

                //        using var ms = new MemoryStream();

                //        cropCopy.Save(ms, new JpegEncoder());
                //        return base.File(ms.ToArray(), "image/jpeg");
                //    }
                //}

                var imageData = imageCollection.FindOne(x => x.ImageType == imageType);
                if (imageData != null)
                {
                    return base.File(imageData.Image, "image/jpeg");
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
            return Ok();
        }

        [HttpGet("DigitalNumbers")]
        public ActionResult<IEnumerable<DigitalNumber>> GetDigitalNumbers()
        {
            var numberCollection = _DbContext.Context.GetCollection<DigitalNumber>();

            try
            {

                return Ok(numberCollection.FindAll().ToList());
            }
            catch (Exception)
            {
                numberCollection.DeleteAll();
            }
            return Ok(Enumerable.Empty<DigitalNumber>());
        }


        [HttpPost("DigitalNumber")]
        public IActionResult PostConfigValue([FromBody] DigitalNumber value)
        {
            var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
            digitalNumberCollection.DeleteMany(x => x.Id == value.Id);
            digitalNumberCollection.Insert(value);
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


                if (image == null || image.Created < DateTime.Now.AddMinutes(-5))
                {
                    var imageSrc = configCollection.FindOne(x => x.Key == ConfigParamters.ImageSrc);
                    using var httpClient = new HttpClient();
                    var imageData = await httpClient.GetByteArrayAsync(imageSrc.Value);

                    if (image != null)
                    {
                        imageCollection.DeleteMany(i => i.ImageType == ImageType.SrcImage);
                    }

                    image = new ImageData
                    {
                        Image = imageData,
                        Created = DateTime.Now,
                        ImageType = ImageType.SrcImage
                    };
                    imageCollection.Insert(image);

                }


                //var configCollection = _DbContext.Context.GetCollection<ConfigValue>();
                var rotate = float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.ImageAngle)?.Value ?? "0");
                var offsetHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropOffsetHorizontal)?.Value ?? "0");
                var offsetVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropOffsetVertical)?.Value ?? "0");
                var sizeHorizontal = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropWidth)?.Value ?? "0");
                var sizeVertical = (int)float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.CropHeight)?.Value ?? "0");

                using var imageToClone = Image.Load(image.Image);
                using var rotateCopy = imageToClone.Clone(i => i.Rotate(RotateMode.Rotate180).Rotate(rotate));


                //TODO Size after Rotate?
                sizeHorizontal = sizeHorizontal <= 0 ? rotateCopy.Width - offsetHorizontal : sizeHorizontal;
                sizeVertical = sizeVertical <= 0 ? rotateCopy.Height - offsetVertical : sizeVertical;

                using var cropCopy = rotateCopy.Clone(i => i.Crop(new Rectangle(offsetHorizontal, offsetVertical, sizeHorizontal, sizeVertical)));

                using var ms = new MemoryStream();
                cropCopy.Save(ms, new JpegEncoder());

                image = new ImageData
                {
                    Image = ms.ToArray(),
                    Created = DateTime.Now,
                    ImageType = ImageType.ProcessedImage
                };
                imageCollection.Insert(image);

                using var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default);

                var digitalNumberCollection = _DbContext.Context.GetCollection<DigitalNumber>();
                imageCollection.DeleteMany(x => x.ImageType == ImageType.Number);
                foreach (var digitalNumber in digitalNumberCollection.FindAll())
                {
                    using var digitalNumberCopy = cropCopy.Clone(i => i.Crop(new Rectangle(digitalNumber.HorizontalOffset, digitalNumber.VerticalOffset, digitalNumber.Width, digitalNumber.Height)));
                    using var digitalNumberStream = new MemoryStream();
                    digitalNumberCopy.Save(digitalNumberStream, new JpegEncoder());

                    var numberImage = new ImageData
                    {
                        Image = digitalNumberStream.ToArray(),
                        Created = DateTime.Now,
                        ImageType = ImageType.Number,
                        Number = digitalNumber.Id
                    };
                    imageCollection.Insert(numberImage);

                    using (var img = Pix.LoadFromMemory(numberImage.Image))
                    {
                        using var page = engine.Process(img, PageSegMode.SingleChar);
                        var content = page.GetText();
                        Console.WriteLine(result);
                        if (int.TryParse(content, out var numericValue))
                        {
                            digitalNumber.Value = numericValue;
                        }
                        else
                        {
                            digitalNumber.Value = 0;
                        }
                    }
                    digitalNumberCollection.Update(digitalNumber);
                }

                result.DigitalNumbers = digitalNumberCollection.FindAll();


            }
            catch (Exception e)
            {
                //return BadRequest(e);
            }

            return result;
        }
    }
}