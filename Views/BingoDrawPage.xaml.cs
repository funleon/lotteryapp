using Microsoft.UI.Xaml.Controls;
using lotteryapp.ViewModels;

namespace lotteryapp.Views;

public sealed partial class BingoDrawPage : Page
{
    public BingoDrawViewModel ViewModel { get; }

    public BingoDrawPage()
    {
        this.InitializeComponent();
        ViewModel = new BingoDrawViewModel();
    }
}
