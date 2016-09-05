using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Data;

namespace Core
{
	public class DynamicObjectClass : DynamicObject, INotifyPropertyChanged
	{
		public bool SimpleTryGetValue(string name, out object result, bool ignoreCase = false)
		{
			if (!ignoreCase)
				return members.TryGetValue(name, out result);
			var p = members.FirstOrDefault(p2 => p2.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			result = p.Value;
			return p.Key != null;
		}

		public void SimpleSetValue(string name, object value)
		{
			members[name] = value;
		}

		#region DynamicObject overrides

		public override bool TryGetMember(GetMemberBinder binder, out object result) => SimpleTryGetValue(binder.Name, out result, binder.IgnoreCase);

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			members[binder.Name] = value;
			NotifyPropertyChanged(binder.Name);
			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return members.Keys;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			int index = (int)indexes[0];
			try
			{
				result = itemsCollection[index];
			}
			catch (ArgumentOutOfRangeException)
			{
				result = null;
				return false;
			}
			return true;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			int index = (int)indexes[0];
			itemsCollection[index] = value;
			NotifyPropertyChanged(System.Windows.Data.Binding.IndexerName);
			return true;
		}

		public override bool TryDeleteMember(DeleteMemberBinder binder)
		{
			if (members.ContainsKey(binder.Name))
			{
				members.Remove(binder.Name);
				return true;
			}
			return false;
		}

		public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
		{
			int index = (int)indexes[0];
			itemsCollection.RemoveAt(index);
			return true;
		}

		#endregion DynamicObject overrides

		#region INotifyPropertyChanged

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

		#endregion INotifyPropertyChanged

		#region Public methods

		public object AddItem(object item)
		{
			itemsCollection.Add(item);
			NotifyPropertyChanged(Binding.IndexerName);
			return null;
		}

		#endregion Public methods

		#region Private data

		Dictionary<string, object> members = new Dictionary<string, object>();
		ObservableCollection<object> itemsCollection = new ObservableCollection<object>();

		#endregion Private data
	}
}
