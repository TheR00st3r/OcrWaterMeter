namespace OcrWaterMeter.Shared
{
    public class ConfigValue
    {
        public ConfigValue(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get;  }

        public string Value { get;  }
    }
}