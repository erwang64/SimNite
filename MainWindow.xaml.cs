using System.Windows;
using SimNite.Services;
using SimNite.ViewModels;

namespace SimNite;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new DatabaseService(), new ProfileService());
    }
}