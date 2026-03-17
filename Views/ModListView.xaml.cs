using System.Windows.Controls;
using SimNite.ViewModels;

namespace SimNite.Views;

public partial class ModListView : UserControl
{
    public ModListView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (DataContext is ModListViewModel viewModel)
        {
            viewModel.LoadModsCommand.Execute(null);
        }
    }
}

