using System.Net.Http.Json;
using OcrWaterMeter.Shared;
using System.Globalization;

namespace OcrWaterMeter.Client.Pages
{
    public partial class Config
    {
        private readonly IList<DigitalNumber> DigitalNumbers = new List<DigitalNumber>();
        private readonly IList<AnalogNumber> AnalogNumbers = new List<AnalogNumber>();

        private string processTime = string.Empty;
        private bool loading = true;
        private WaterMeterDebugData? waterMeter;
        private IEnumerable<ConfigValue> configValues = Enumerable.Empty<ConfigValue>();
        private string version = string.Empty;
        private string build = string.Empty;
        private string commit = string.Empty;

        #region ConfigValue Properties

        private string _ImageSrc = string.Empty;
        private string ImageSrc
        {
            get => _ImageSrc;
            set
            {
                _ImageSrc = value;
                _ = UpdateValue(ConfigParameters.ImageSrc, value.ToString());
            }
        }

        private decimal _InitialValue = 0;
        private decimal InitialValue
        {
            get => _InitialValue;
            set
            {
                _InitialValue = value;
                _ = UpdateValue(ConfigParameters.InitialValue, value.ToString());
            }
        }

        private decimal _MaxWaterPerHour = 0;
        private decimal MaxWaterPerHour
        {
            get => _MaxWaterPerHour;
            set
            {
                _MaxWaterPerHour = value;
                _ = UpdateValue(ConfigParameters.MaxWaterPerHour, value.ToString());
            }
        }

        private float _ImageAngle;
        private float ImageAngle
        {
            get => _ImageAngle;
            set
            {
                _ImageAngle = value;
                _ = UpdateValue(ConfigParameters.ImageAngle, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropOffsetHorizontal;
        private float CropOffsetHorizontal
        {
            get => _CropOffsetHorizontal;
            set
            {
                _CropOffsetHorizontal = value;
                _ = UpdateValue(ConfigParameters.CropOffsetHorizontal, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropOffsetVertical;
        private float CropOffsetVertical
        {
            get => _CropOffsetVertical;
            set
            {
                _CropOffsetVertical = value;
                _ = UpdateValue(ConfigParameters.CropOffsetVertical, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropWidth;
        private float CropWidth
        {
            get => _CropWidth;
            set
            {
                _CropWidth = value;
                _ = UpdateValue(ConfigParameters.CropWidth, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _CropHeight;
        private float CropHeight
        {
            get => _CropHeight;
            set
            {
                _CropHeight = value;
                _ = UpdateValue(ConfigParameters.CropHeight, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _Lightness;
        private float Lightness
        {
            get => _Lightness;
            set
            {
                _Lightness = value;
                _ = UpdateValue(ConfigParameters.Lightness, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private float _Contrast;
        private float Contrast
        {
            get => _Contrast;
            set
            {
                _Contrast = value;
                _ = UpdateValue(ConfigParameters.Contrast, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private byte _AnalogColorR;
        private byte AnalogColorR
        {
            get => _AnalogColorR;
            set
            {
                _AnalogColorR = value;
                _ = UpdateValue(ConfigParameters.AnalogColorR, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private byte _AnalogColorG;
        private byte AnalogColorG
        {
            get => _AnalogColorG;
            set
            {
                _AnalogColorG = value;
                _ = UpdateValue(ConfigParameters.AnalogColorG, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private byte _AnalogColorB;
        private byte AnalogColorB
        {
            get => _AnalogColorB;
            set
            {
                _AnalogColorB = value;
                _ = UpdateValue(ConfigParameters.AnalogColorB, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        protected override async Task OnInitializedAsync()
        {
            configValues = await Http.GetFromJsonAsync<IEnumerable<ConfigValue>>("WaterMeter/ConfigValues") ?? Enumerable.Empty<ConfigValue>();
            if (configValues.Any())
            {
                _ImageSrc = configValues.FirstOrDefault(x => x.Key == ConfigParameters.ImageSrc)?.Value ?? string.Empty;
                _InitialValue = decimal.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.InitialValue)?.Value ?? "0", CultureInfo.InvariantCulture);
                _ImageAngle = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.ImageAngle)?.Value ?? "0", CultureInfo.InvariantCulture);
                _CropOffsetHorizontal = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.CropOffsetHorizontal)?.Value ?? "0", CultureInfo.InvariantCulture);
                _CropOffsetVertical = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.CropOffsetVertical)?.Value ?? "0", CultureInfo.InvariantCulture);
                _CropWidth = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.CropWidth)?.Value ?? "0", CultureInfo.InvariantCulture);
                _CropHeight = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.CropHeight)?.Value ?? "0", CultureInfo.InvariantCulture);
                _Lightness = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.Lightness)?.Value ?? "1", CultureInfo.InvariantCulture);
                _Contrast = float.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.Contrast)?.Value ?? "1", CultureInfo.InvariantCulture);
                _AnalogColorR = byte.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.AnalogColorR)?.Value ?? "150", CultureInfo.InvariantCulture);
                _AnalogColorG = byte.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.AnalogColorG)?.Value ?? "50", CultureInfo.InvariantCulture);
                _AnalogColorB = byte.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.AnalogColorB)?.Value ?? "50", CultureInfo.InvariantCulture);
                _MaxWaterPerHour = decimal.Parse(configValues.FirstOrDefault(x => x.Key == ConfigParameters.MaxWaterPerHour)?.Value ?? "4", CultureInfo.InvariantCulture);
            }

            version = await Http.GetStringAsync("version");
            build = await Http.GetStringAsync("build");
            commit = await Http.GetStringAsync("commit");

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
            processTime = DateTime.Now.Ticks.ToString();
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
            processTime = DateTime.Now.Ticks.ToString();
        }

        private async void DigitalNumberPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Http.PostAsJsonAsync("WaterMeter/DigitalNumber", sender as DigitalNumber);
            await UpdateData();
            processTime = DateTime.Now.Ticks.ToString();
        }

        private async void AnalogNumberPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Http.PostAsJsonAsync("WaterMeter/AnalogNumber", sender as AnalogNumber);
            await UpdateData();
            processTime = DateTime.Now.Ticks.ToString();
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