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



        [HttpGet("Image/{type}")]
        public ActionResult GetImage(string type)
        {
            if (Enum.TryParse<ImageType>(type, out var imageType))
            {
                var imageCollection = _DbContext.Context.GetCollection<ImageData>();
                if (imageType == ImageType.ProcessedImage)
                {
                    var imageDataToClone = imageCollection.FindOne(x => x.ImageType == ImageType.SrcImage);
                    if (imageDataToClone != null)
                    {
                        var configCollection = _DbContext.Context.GetCollection<ConfigValue>();
                        var rotate = float.Parse(configCollection.FindOne(x => x.Key == ConfigParamters.ImageAngle)?.Value ?? "0");

                        using (var image = Image.Load(imageDataToClone.Image))
                        {
                            using (Image copy = image.Clone(i => i.Rotate(RotateMode.Rotate180).Rotate(rotate)))
                            {
                                using (var ms = new MemoryStream())
                                {
                                    copy.Save(ms, new JpegEncoder());
                                    return base.File(ms.ToArray(), "image/jpeg");
                                }
                            }
                        }
                    }

                }
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

                result.Image = image;

                using (var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(image.Image))
                    {
                        var page = engine.Process(img);
                        var content = page.GetText();
                        Console.WriteLine(result);
                    }
                }

            }
            catch (Exception e)
            {
                //return BadRequest(e);
            }

            return result;
        }
    }
}