using System.Text.RegularExpressions;
using OfficeOpenXml;
using Utgifter.Api.Models;
using Utgifter.Api.Extensions;

namespace Utgifter.Api.Features.Expenses.Upload.ExcelParsers;

public partial class TransactionListParser(ExcelWorksheet sheet)
{
    private int _row = 1;
    private State _state = State.WaitingForUser;
    private readonly List<Expense> _expenses = [];
    private string? _user;
    private enum State
    {
        WaitingForUser,
        ReadingUser,
        ReadingTransactions,
        EndOfFile
    }

    public Expense[] Parse()
    {
        while (true)
        {
            switch (_state)
            {
                case State.WaitingForUser:
                    WaitingForUser();
                    break;
                case State.ReadingUser:
                    ReadUser();
                    break; 
                case State.ReadingTransactions:
                    ReadingTransactions();
                    break;
                case State.EndOfFile:
                    return _expenses.ToArray();
            }
        }
    }

    
    
    private void ReadingTransactions()
    {
        var initialCell = sheet.Cells[_row, 1].Text;
        if (initialCell.Equals("Totalbeløp", StringComparison.OrdinalIgnoreCase))
        {
            _state = State.WaitingForUser;
            _row += 2;
            return;
        }
        
        if (initialCell.StartsWith("Valutakurs", StringComparison.OrdinalIgnoreCase))
        {
            _row++;
            return;
        }
        
        var dateString = sheet.Cells[_row, 1].GetValue<string>();

        if (!DateOnly.TryParseExact(dateString, "dd/MM/yyyy", out var date)
            && !DateOnly.TryParseExact(dateString, "d.M.yyyy", out date))
        {
            throw new Exception($"Invalid date: {dateString} on column {_row} row ");
        }
        
        var store = sheet.Cells[_row, 3].GetValue<string>()
            .TrimStart()
            .TrimStart(StringComparison.OrdinalIgnoreCase, "VIPPS*", "ZETTLE_*", "SUMUP  *", "NYX*", "MS*", "KLARNA*")
            .Trim();
        var city = sheet.Cells[_row, 4].GetValue<string>();
        var currency = sheet.Cells[_row, 5].GetValue<string>();
        var amount = sheet.Cells[_row, 6].GetValue<decimal>();
        var isOriginalCurrency = currency.Equals("nok", StringComparison.OrdinalIgnoreCase);

        _expenses.Add(Expense.New(date, _user ?? throw new ArgumentException("No user in file"),store, city, currency, amount, trip: isOriginalCurrency ? null : "UNKNOWN"));
        _row++;
    }
    

    private void ReadUser()
    {
        _user = sheet.Cells[_row, 2].Text;
        _state = State.ReadingTransactions;
        _row += 3;
    }

    [GeneratedRegex(@"\d+\*+\d+")]
    private static partial Regex CreditCardNumber();
    private void WaitingForUser()
    {
        var cell = sheet.Cells[_row,1].Text;
        if (CreditCardNumber().IsMatch(cell))
        {
            _state = State.ReadingUser;
            return;
        }

        if (_expenses.Count != 0) 
            _state = State.EndOfFile;
        _row++;
    }
    
}