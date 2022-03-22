using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OcrWaterMeter.Shared
{
    public class NumberBase : INotifyPropertyChanged
    {
        private int _HorizontalOffset;
        private int _VverticalOffset;
        private int _Width;
        private int _Height;
        private int _OcrValue;
        private int _Value;
        private int _LastValue;
        private decimal _Factor = 1;

        public int Id { get; set; }

        public int HorizontalOffset
        {
            get => _HorizontalOffset;
            set
            {
                _HorizontalOffset = value;
                OnPropertyChanged();
            }
        }



        public int VerticalOffset
        {
            get => _VverticalOffset;
            set
            {
                _VverticalOffset = value;
                OnPropertyChanged();
            }
        }

        public int Width
        {
            get => _Width;
            set
            {
                _Width = value;
                OnPropertyChanged();
            }
        }

        public int Height
        {
            get => _Height;
            set
            {
                _Height = value;
                OnPropertyChanged();
            }
        }

        public int OcrValue
        {
            get => _OcrValue;
            set
            {
                _OcrValue = value;
                OnPropertyChanged();
            }
        }

        public int Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }

        public int LastValue
        {
            get => _LastValue;
            set
            {
                _LastValue = value;
                OnPropertyChanged();
            }
        }

        public decimal Factor
        {
            get => _Factor;
            set
            {
                _Factor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DigitalNumber : NumberBase
    {

    }

    public class AnalogNumber : NumberBase
    {
        private byte _CenterColorR;
        private byte _CenterColorG;
        private byte _CenterColorB;

        public byte CenterColorR
        {
            get => _CenterColorR;
            set
            {
                _CenterColorR = value;
                OnPropertyChanged();
            }
        }

        public byte CenterColorG
        {
            get => _CenterColorG;
            set
            {
                _CenterColorG = value;
                OnPropertyChanged();
            }
        }

        public byte CenterColorB
        {
            get => _CenterColorB;
            set
            {
                _CenterColorB = value;
                OnPropertyChanged();
            }
        }
    }
}
