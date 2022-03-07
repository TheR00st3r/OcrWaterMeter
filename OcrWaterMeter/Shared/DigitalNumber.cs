using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OcrWaterMeter.Shared
{
    public class DigitalNumber : INotifyPropertyChanged
    {
        private int _HorizontalOffset;
        private int _VverticalOffset;
        private int _Width;
        private int _Height;
        private int _Value;

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

        public int Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
