using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lotteryapp.Models;
using lotteryapp.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace lotteryapp.ViewModels;

public partial class PrizeDrawViewModel : ObservableObject
{
    private readonly LotteryService _lotteryService;
    private readonly DispatcherQueue _dispatcherQueue;

    private int GetSecureRandomInt(int maxValue)
    {
        if (maxValue <= 0) return 0;
        return RandomNumberGenerator.GetInt32(maxValue);
    }

    [ObservableProperty]
    private string title = "獎品抽獎 Prize Draw";

    [ObservableProperty]
    private ObservableCollection<Prize> prizes = new();

    [ObservableProperty]
    private Prize? selectedPrize;

    [ObservableProperty]
    private int drawCount = 1;

    [ObservableProperty]
    private ObservableCollection<Winner> winners = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSelectPrize))]
    [NotifyCanExecuteChangedFor(nameof(DrawCommand))]
    private bool isRolling;

    public bool CanSelectPrize => !IsRolling;

    [ObservableProperty]
    private string currentRollingName = "等待抽獎..."; // Default text

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private int totalPrizes;

    [ObservableProperty]
    private int wonPrizes;

    public PrizeDrawViewModel()
    {
        _lotteryService = new LotteryService();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var prizeList = _lotteryService.GetPrizeList();
            Prizes = new ObservableCollection<Prize>(prizeList);
            if (Prizes.Any())
            {
                SelectedPrize = Prizes.First();
                UpdateWinnersList();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
    }

    partial void OnSelectedPrizeChanged(Prize? value)
    {
        if (value != null)
        {
            TotalPrizes = value.Num;
            DrawCount = TotalPrizes; // Default to TotalPrizes as requested
        }
        StatusMessage = string.Empty;
        CurrentRollingName = "等待抽獎...";
        UpdateWinnersList();
    }

    private void UpdateWinnersList()
    {
        if (SelectedPrize == null)
        {
            Winners.Clear();
            return;
        }

        var allWinners = _lotteryService.GetWinners();
        var prizeWinners = allWinners.Where(w => w.PrizeName == SelectedPrize.PrizeName).OrderBy(w => w.WinOrder).ToList();
        Winners = new ObservableCollection<Winner>(prizeWinners);
        WonPrizes = prizeWinners.Count;
    }

    [RelayCommand(CanExecute = nameof(CanSelectPrize))]
    private async Task Draw()
    {
        if (SelectedPrize == null)
        {
            StatusMessage = "請選擇獎項。";
            return;
        }


        int remainingPrizes = TotalPrizes - WonPrizes;
        if (remainingPrizes <= 0)
        {
            StatusMessage = "此獎項已全數抽出。";
            return;
        }

        if (DrawCount <= 0)
        {
            StatusMessage = "抽取人數必須大於 0。";
            return;
        }

        if (DrawCount > remainingPrizes)
        {
            StatusMessage = $"抽取人數不能超過剩餘獎項數量 ({remainingPrizes})。";
            return;
        }

        IsRolling = true;
        StatusMessage = "";

        // Get candidates
        var allPeople = _lotteryService.GetPersonList();
        var allWinners = _lotteryService.GetWinners();
        var winnerIds = new HashSet<string>(allWinners.Select(w => w.Id));

        var candidates = allPeople.Where(p => !winnerIds.Contains(p.ID)).ToList();

        if (!candidates.Any())
        {
            StatusMessage = "沒有符合資格的候選人。";
            IsRolling = false;
            CurrentRollingName = "無候選人";
            return;
        }

        // Animation
        var endTime = DateTime.Now.AddSeconds(3); // Roll for 3 seconds
        while (DateTime.Now < endTime)
        {
            var randomPerson = candidates[GetSecureRandomInt(candidates.Count)];
            var display = $"{randomPerson.DeptId} - {randomPerson.Name}";
            
            // UI update needs to be on UI thread
            _dispatcherQueue.TryEnqueue(() => CurrentRollingName = display);
            
            await Task.Delay(50); // Speed of rolling
        }

        // Draw Logic
        int countToDraw = Math.Min(DrawCount, candidates.Count);
        
        for (int i = 0; i < countToDraw; i++)
        {
            // Re-evaluate candidates in case we are drawing multiple and want to avoid duplicates in the same batch
            // Though for simplicity in this loop, we just pick from remaining candidates
             if (!candidates.Any()) break;

            int index = GetSecureRandomInt(candidates.Count);
            var winnerPerson = candidates[index];
            candidates.RemoveAt(index); // Remove so distinct for this batch

            var winner = new Winner
            {
                DeptId = winnerPerson.DeptId,
                Id = winnerPerson.ID,
                Name = winnerPerson.Name,
                PrizeName = SelectedPrize.PrizeName,
                WinOrder = Winners.Count + 1
            };

            _lotteryService.SaveWinner(winner);
            
            _dispatcherQueue.TryEnqueue(() => 
            {
                Winners.Add(winner);
                CurrentRollingName = $"{winner.DeptId} - {winner.Name}";
                WonPrizes++;
            });
            
            // Add a small delay to visualize each winner being drawn and ensure the last one is displayed
            await Task.Delay(100);
        }
        
        
        IsRolling = false;
        StatusMessage = $"已抽出 {countToDraw} 位得獎者。";
    }

    [RelayCommand]
    private void ResetMessage()
    {
        StatusMessage = string.Empty;
    }
}
