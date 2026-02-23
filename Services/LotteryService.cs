using ClosedXML.Excel;
using lotteryapp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lotteryapp.Services;

public class LotteryService
{
    private const string DrawListFile = "drawlist.xlsx";
    private const string PrizeFile = "prize.xlsx";
    private const string WinnerFile = "winner.xlsx";
    private const string BingoFile = "bingo.xlsx";

    // 讀取抽獎名單
    public List<DrawPerson> GetPersonList()
    {
        var list = new List<DrawPerson>();
        if (!File.Exists(DrawListFile)) return list;

        try
        {
            using var workbook = new XLWorkbook(DrawListFile);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();
            if (range == null) return list;
            var rows = range.RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                var name = row.Cell(1).GetValue<string>();
                var id = row.Cell(2).GetValue<string>()?.Trim();
                var deptId = row.Cell(3).GetValue<string>();

                // Input Validation: Skip invalid rows
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                list.Add(new DrawPerson
                {
                    Name = name, 
                    ID = id,   
                    DeptId = deptId
                });
            }
        }
        catch (Exception ex)
        {
            // Log or handle error
            throw new Exception($"Error reading drawlist.xlsx: {ex.Message}");
        }

        return list;
    }

    // 讀取獎項清單
    public List<Prize> GetPrizeList()
    {
        var list = new List<Prize>();
        if (!File.Exists(PrizeFile)) return list;

        try
        {
            using var workbook = new XLWorkbook(PrizeFile);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();
            if (range == null) return list;
            var rows = range.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var prizeName = row.Cell(2).GetValue<string>();
                
                // Input Validation
                if (string.IsNullOrWhiteSpace(prizeName)) continue;

                list.Add(new Prize
                {
                    DrawOrder = row.Cell(1).GetValue<int>(), // DRAWORDER
                    PrizeName = prizeName, // PRIZENAME
                    Num = row.Cell(3).GetValue<int>() // NUM
                });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading prize.xlsx: {ex.Message}");
        }

        return list.OrderBy(p => p.DrawOrder).ToList();
    }

    // 讀取中獎人清單
    public List<Winner> GetWinners()
    {
        var list = new List<Winner>();
        if (!File.Exists(WinnerFile)) return list;

        try
        {
            using var workbook = new XLWorkbook(WinnerFile);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();
            if (range == null) return list;
            var rows = range.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                list.Add(new Winner
                {
                    DeptId = row.Cell(1).GetValue<string>(),
                    Id = row.Cell(2).GetValue<string>().Trim(), // ID - Trimmed
                    Name = row.Cell(3).GetValue<string>(),
                    PrizeName = row.Cell(4).GetValue<string>(),
                    WinOrder = row.Cell(5).GetValue<int>()
                });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading winner.xlsx: {ex.Message}");
        }

        return list;
    }

    // 儲存中獎人
    public void SaveWinner(Winner winner)
    {
        try
        {
            XLWorkbook workbook;
            IXLWorksheet worksheet;

            if (File.Exists(WinnerFile))
            {
                workbook = new XLWorkbook(WinnerFile);
                worksheet = workbook.Worksheet(1);
            }
            else
            {
                workbook = new XLWorkbook();
                worksheet = workbook.Worksheets.Add("Winners");
                // Header
                worksheet.Cell(1, 1).Value = "DEPTID";
                worksheet.Cell(1, 2).Value = "ID";
                worksheet.Cell(1, 3).Value = "NAME";
                worksheet.Cell(1, 4).Value = "PRIZENAME";
                worksheet.Cell(1, 5).Value = "WINORDER";
            }

            var nextRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            worksheet.Cell(nextRow, 1).Value = winner.DeptId;
            // Force ID to be text to avoid auto conversion of "001" to 1
            worksheet.Cell(nextRow, 2).Value = "'" + winner.Id; 
            worksheet.Cell(nextRow, 3).Value = winner.Name;
            worksheet.Cell(nextRow, 4).Value = winner.PrizeName;
            worksheet.Cell(nextRow, 5).Value = winner.WinOrder;

            workbook.SaveAs(WinnerFile);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving winner.xlsx: {ex.Message}");
        }
    }

    // 儲存賓果號碼
    public void SaveBingoNumber(int number)
    {
        try
        {
            XLWorkbook workbook;
            IXLWorksheet worksheet;

            if (File.Exists(BingoFile))
            {
                workbook = new XLWorkbook(BingoFile);
                worksheet = workbook.Worksheet(1);
            }
            else
            {
                workbook = new XLWorkbook();
                worksheet = workbook.Worksheets.Add("Bingo");
                worksheet.Cell(1, 1).Value = "NUMBER";
                worksheet.Cell(1, 2).Value = "DRAWNAT";
            }

            var nextRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
            worksheet.Cell(nextRow, 1).Value = number;
            worksheet.Cell(nextRow, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            workbook.SaveAs(BingoFile);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving bingo.xlsx: {ex.Message}");
        }
    }

    // 讀取已抽出的賓果號碼 (為了恢復狀態)
    public List<int> GetBingoNumbers()
    {
        var list = new List<int>();
        if (!File.Exists(BingoFile)) return list;

        try
        {
            using var workbook = new XLWorkbook(BingoFile);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();
            if (range == null) return list;
            var rows = range.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                if (row.Cell(1).TryGetValue<int>(out var number))
                {
                    list.Add(number);
                }
            }
        }
        catch
        {
            // Ignore errors for bingo read
        }
        return list;
    }
}
