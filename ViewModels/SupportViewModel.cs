using System.Windows.Input;

namespace SimNite.ViewModels;

public class SupportViewModel : BaseViewModel
{
    public SupportViewModel()
    {
        OpenPayPalCommand = new RelayCommand(_ => OpenUrl("https://www.paypal.com/paypalme/Erwangim"));
        OpenBuyMeACoffeeCommand = new RelayCommand(_ => OpenUrl("https://www.buymeacoffee.com/erwangimenez"));
    }

    public ICommand OpenPayPalCommand { get; }
    public ICommand OpenBuyMeACoffeeCommand { get; }

    private void OpenUrl(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { /* Ignore */ }
    }
}