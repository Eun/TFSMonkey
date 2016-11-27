using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using FontAwesome.WPF;

namespace TFSMonkey
{
	public class NameColorIcon : INotifyPropertyChanged
	{
		private int _count;
		private string _name;
		private Color _color;
		private string _icon;

		public string Icon
		{
			get { return _icon; }
			set
			{
				_icon = value;
				OnPropertyChanged(nameof(Icon));
			}
		}

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				OnPropertyChanged(nameof(Color));
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public int Count
		{
			get { return _count; }
			set
			{
				_count = value;
				OnPropertyChanged(nameof(Count));
			}
		}

		public NameColorIcon(string name, Color color, string icon)
		{
			this.Name = name;
			this.Color = color;
			this.Icon = icon;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}