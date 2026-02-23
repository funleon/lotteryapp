using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lotteryapp.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace lotteryapp.ViewModels;

public partial class BingoDrawViewModel : ObservableObject
{
    private readonly LotteryService _lotteryService;
    private readonly DispatcherQueue _dispatcherQueue;

    private int GetSecureRandomInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue) return minValue;
        return RandomNumberGenerator.GetInt32(minValue, maxValue);
    }

    [ObservableProperty]
    private string title = "賓果抽獎 Bingo Draw";

    // --- 實際使用的設定 ---
    [ObservableProperty]
    private int minNum = 1;

    [ObservableProperty]
    private int maxNum = 75;

    [ObservableProperty]
    private int drawCount = 1;

    // --- 畫面上綁定的暫存設定 ---
    [ObservableProperty]
    private int inputMinNum = 1;

    [ObservableProperty]
    private int inputMaxNum = 75;

    [ObservableProperty]
    private int inputDrawCount = 1;

    [ObservableProperty]
    private ObservableCollection<int> drawnNumbers = new();

    [ObservableProperty]
    private string currentRollingNumber = "?";

    [ObservableProperty]
    private bool isRolling;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [RelayCommand]
    private void ApplySettings()
    {
        if (InputMinNum >= InputMaxNum)
        {
            StatusMessage = "套用失敗：最小數字必須小於最大數字。";
            return;
        }
        if (InputDrawCount <= 0)
        {
            StatusMessage = "套用失敗：抽取個數必須大於0。";
            return;
        }

        MinNum = InputMinNum;
        MaxNum = InputMaxNum;
        DrawCount = InputDrawCount;
        StatusMessage = $"設定已套用: 範圍 {MinNum}~{MaxNum}，每次抽取 {DrawCount} 個。";
    }

    public BingoDrawViewModel()
    {
        _lotteryService = new LotteryService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        LoadData();
    }

    private void LoadData()
    {
        var history = _lotteryService.GetBingoNumbers();
        DrawnNumbers = new ObservableCollection<int>(history);
    }

    [RelayCommand]
    private async Task Draw()
    {
        if (MinNum >= MaxNum)
        {
            StatusMessage = "最小數字必須小於最大數字。";
            return;
        }

        StatusMessage = "";
        IsRolling = true;

        // Animation Loop
        var endTime = DateTime.Now.AddSeconds(2);
        while (DateTime.Now < endTime)
        {
            var tempNum = GetSecureRandomInt(MinNum, MaxNum + 1);
            _dispatcherQueue.TryEnqueue(() => CurrentRollingNumber = tempNum.ToString());
            await Task.Delay(50);
        }

        // Draw Logic
        // Loop for DrawCount
        int successfulDraws = 0;
        for (int i = 0; i < DrawCount; i++)
        {
             // Find a number that hasn't been drawn
            int drawn = -1;
            int attempts = 0;
            int maxAttempts = 1000; // Prevent infinite loop

            while (attempts < maxAttempts)
            {
                int candidate = GetSecureRandomInt(MinNum, MaxNum + 1);
                if (!DrawnNumbers.Contains(candidate))
                {
                    drawn = candidate;
                    break;
                }
                attempts++;
            }

            if (drawn == -1)
            {
                IsRolling = false;
                CurrentRollingNumber = "END";
                StatusMessage = $"區間內所有號碼已抽出 (共抽出 {successfulDraws} 個)。";
                return;
            }

            _lotteryService.SaveBingoNumber(drawn);
            successfulDraws++;

            _dispatcherQueue.TryEnqueue(() =>
            {
                DrawnNumbers.Insert(0, drawn); // Add to top
                CurrentRollingNumber = drawn.ToString();
            });
            
            // Small delay between draws if multiple
            if (DrawCount > 1) await Task.Delay(200);
        }

        IsRolling = false;
    }

    [RelayCommand]
    private void Reset()
    {
        // Optional: Clear logic if needed, but for now we just clear message
        StatusMessage = "";
    }
}
