using System;
using System.ComponentModel;

namespace Core
{
	public class BindableBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void NotifyPropertyChanged(String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public virtual void NotifyPropertiesChanged(params String[] propertyNames)
		{
			if (PropertyChanged != null)
			{
				foreach (var prop in propertyNames)
					PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}
		}

		public virtual void Route(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, e);
		}
	}
}
