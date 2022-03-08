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
        private readonly IList<DigitalNumber> DigitalNumbers = new List<DigitalNumber>();
        private readonly IList<AnalogNumber> AnalogNumbers = new List<AnalogNumber>();

        private string ProcessTime = string.Empty;
        private bool loading = true;
        private WaterMeterDebugData? waterMeter;
        private IEnumerable<ConfigValue> configValues = Enumerable.Empty<ConfigValue>();

        private string _ImageSrc = string.Empty;
        private string ImageSrc
        {
            get => _ImageSrc;
            set
            {
                _ImageSrc = value;
                _ = UpdateValue(ConfigParamters.ImageSrc, value.ToString());
            }
        }

        private float _ImageAngle;
        private float ImageAngle
        {
            get => _ImageAngle;
            set
            {
                _ImageAngle = value;
                _ = UpdateValue(ConfigParamters.ImageAngle, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropOffsetHorizontal;
        private float CropOffsetHorizontal
        {
            get => _CropOffsetHorizontal;
            set
            {
                _CropOffsetHorizontal = value;
                _ = UpdateValue(ConfigParamters.CropOffsetHorizontal, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropOffsetVertical;
        private float CropOffsetVertical
        {
            get => _CropOffsetVertical;
            set
            {
                _CropOffsetVertical = value;
                _ = UpdateValue(ConfigParamters.CropOffsetVertical, value.ToString(CultureInfo.InvariantCulture));
            }
        }


        private float _CropWidth;
        private float CropWidth
        {
            get => _CropWidth;
            set
            {
                _CropWidth = value;
                _ = UpdateValue(ConfigParamters.CropWidth, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropHeight;
        private float CropHeight
        {
            get => _CropHeight;
            set
            {
                _CropHeight = value;
                _ = UpdateValue(ConfigParamters.CropHeight, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        protected override async Task OnInitializedAsync()
        {
            configValues = await Http.GetFromJsonAsync<IEnumerable<ConfigValue>>("WaterMeter/ConfigValues") ?? Enumerable.Empty<ConfigValue>();
            if (configValues.Any())
            {
                _ImageSrc = configValues.FirstOrDefault(x => x.Key == ConfigParamters.ImageSrc)?.Value ?? string.Empty;
                _ImageAngle = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.ImageAngle)?.Value ?? "0");
                _CropOffsetHorizontal = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.CropOffsetHorizontal)?.Value ?? "0");
                _CropOffsetVertical = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.CropOffsetVertical)?.Value ?? "0");
                _CropWidth = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.CropWidth)?.Value ?? "0");
                _CropHeight = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParamters.CropHeight)?.Value ?? "0");
            }

            await UpdateData();
            loading = false;
            //StateHasChanged();
        }

        private async Task UpdateData()
        {
            waterMeter = await Http.GetFromJsonAsync<WaterMeterDebugData>("WaterMeter/DebugData");

            DigitalNumbers.Clear();
            if (waterMeter?.DigitalNumbers != null)
            {
                foreach (var digitalNumber in waterMeter.DigitalNumbers)
                {
                    digitalNumber.PropertyChanged += DigitalNumberPropertyChanged;
                    DigitalNumbers.Add(digitalNumber);
                }
            }

            AnalogNumbers.Clear();
            if (waterMeter?.AnalogNumbers != null)
            {
                foreach (var analogNumber in waterMeter.AnalogNumbers)
                {
                    analogNumber.PropertyChanged += AnalogNumberPropertyChanged;
                    AnalogNumbers.Add(analogNumber);
                }
            }

            StateHasChanged();
        }

        private async Task UpdateValue(string key, string value)
        {
            await Http.PostAsJsonAsync("WaterMeter/ConfigValue", new ConfigValue(key, value));
            await UpdateData();
            ProcessTime = DateTime.Now.Ticks.ToString();
        }

        private void AddNewDigitalNumber()
        {
            var newNumber = new DigitalNumber { Id = GetNextFreeNumber() };
            newNumber.PropertyChanged += DigitalNumberPropertyChanged;
            DigitalNumbers.Add(newNumber);
        }

        private void AddNewAnalogNumber()
        {
            var newNumber = new AnalogNumber { Id = GetNextFreeNumber() };
            newNumber.PropertyChanged += AnalogNumberPropertyChanged;
            AnalogNumbers.Add(newNumber);
        }

        private async void RemoveNumber(int id)
        {
            await Http.DeleteAsync($"WaterMeter/Number/{id}");
            await UpdateData();
            ProcessTime = DateTime.Now.Ticks.ToString();
        }

        private async void DigitalNumberPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Http.PostAsJsonAsync("WaterMeter/DigitalNumber", sender as DigitalNumber);
            await UpdateData();
            ProcessTime = DateTime.Now.Ticks.ToString();
        }

        private async void AnalogNumberPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Http.PostAsJsonAsync("WaterMeter/AnalogNumber", sender as AnalogNumber);
            await UpdateData();
            ProcessTime = DateTime.Now.Ticks.ToString();
        }

        private int GetNextFreeNumber()
        {
            for (var i = 1; i < int.MaxValue; i++)
            {
                if (DigitalNumbers.FirstOrDefault(x => x.Id == i) == null && AnalogNumbers.FirstOrDefault(x => x.Id == i) == null)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}