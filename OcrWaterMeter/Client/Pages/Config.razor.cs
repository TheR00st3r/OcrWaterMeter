using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using OcrWaterMeter.Client;
using OcrWaterMeter.Client.Shared;
using OcrWaterMeter.Shared;
using System.Globalization;

namespace OcrWaterMeter.Client.Pages
{
    public partial class Config
    {
        private string processTime;
        private bool loading = true;
        private WaterMeterDebugData waterMeter;
        private string previewImage;
        private IEnumerable<ConfigValue> configValues;
        private string _ImageSrc;
        private string ImageSrc
        {
            get => _ImageSrc;
            set
            {
                _ImageSrc = value;
                UpdateValue(ConfigParamters.ImageSrc, value.ToString());
            }
        }

        private float _ImageAngle;
        private float ImageAngle
        {
            get => _ImageAngle;
            set
            {
                _ImageAngle = value;
                UpdateValue(ConfigParamters.ImageAngle, value.ToString(CultureInfo.InvariantCulture));
                processTime = DateTime.Now.Ticks.ToString();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            configValues = await Http.GetFromJsonAsync<IEnumerable<ConfigValue>>("WaterMeter/ConfigValues");
            _ImageSrc = configValues.FirstOrDefault(x => x.Key == ConfigParamters.ImageSrc)?.Value;
            _ImageAngle = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.ImageAngle)?.Value ?? "0");
            waterMeter = await Http.GetFromJsonAsync<WaterMeterDebugData>("WaterMeter/DebugData");
            var imageSrc = Convert.ToBase64String(waterMeter.Image.Image);
            previewImage = string.Format("data:image/jpeg;base64,{0}", imageSrc);
            loading = false;
        //StateHasChanged();
        }

        private async Task UpdateValue(string key, string value)
        {
            await Http.PostAsJsonAsync("WaterMeter/ConfigValue", new ConfigValue{Key = key, Value = value});
        }
    }
}