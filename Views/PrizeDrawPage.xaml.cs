using Microsoft.UI.Xaml.Controls;
using lotteryapp.ViewModels;

namespace lotteryapp.Views;

public sealed partial class PrizeDrawPage : Page
{
    public PrizeDrawViewModel ViewModel { get; }

    public PrizeDrawPage()
    {
        this.InitializeComponent();
        ViewModel = new PrizeDrawViewModel();
    }
}
