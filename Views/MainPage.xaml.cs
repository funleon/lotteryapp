using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace lotteryapp.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        
        // Navigate frames to their respective pages
        PrizeDrawFrame.Navigate(typeof(PrizeDrawPage));
        BingoDrawFrame.Navigate(typeof(BingoDrawPage));
        AboutFrame.Navigate(typeof(AboutPage));
    }
}
