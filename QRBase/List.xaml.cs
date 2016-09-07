using System.Windows.Controls;
using System.Windows.Input;
using QRBase.Models;
using Ui.Controls;
using System.Linq;
using Core;

namespace Views
{
	/// <summary>
	/// Логика взаимодействия для List.xaml
	/// </summary>
	public partial class List : UserControl
	{
		GridView gridView;
		TileView tileView;
		GaleryView galeryView;
		public List()
		{
			InitializeComponent();
			//CommandBinding search = new CommandBinding(Views.Commands.Search);
			//search.Executed += Search_Executed;
			//search.CanExecute += Search_CanExecute;
			//this.CommandBindings.Add(search);
			gridView = (GridView)FindResource("Grid");
			tileView = (TileView)FindResource("Tile");
			galeryView = (GaleryView)FindResource("Galery");
		}

		//private void Search_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = StaticData.DataViewModel.SearchCommand.CanExecute(null);

		//private void Search_Executed(object sender, ExecutedRoutedEventArgs e) => StaticData.DataViewModel.SearchCommand.Execute(Filter.Text);

		private void Filter_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == Key.Enter)
			//    Commands.Search.Execute(null, null);
		}

		private void LV_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
		{
			//StaticData.PreviewTasks.CancelTask(((DataItem)e.Value).Md5);
		}

		private void btnList_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			LV.View = gridView;
		}

		private void btnTile_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			LV.View = tileView;
		}

		private void btnGalery_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			LV.View = galeryView;
		}

		private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			MainVM.Instance.SelectedQRList = LV.SelectedItems.OfType<QRData>().ToArray();
		}
	}
}
