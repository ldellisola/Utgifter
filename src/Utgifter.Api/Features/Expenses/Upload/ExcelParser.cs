using System.Text.RegularExpressions;
using OfficeOpenXml;
using Utgifter.Api.Models;
using Utgifter.Api.Extensions;

namespace Utgifter.Api.Features.Expenses.Upload;

public partial class ExcelParser(ExcelWorksheet sheet)
{
    private int _column = 1;
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
        var initialCell = sheet.Cells[_column, 1].Text;
        if (initialCell.Equals("Total sum", StringComparison.OrdinalIgnoreCase))
        {
            _state = State.WaitingForUser;
            _column += 2;
            return;
        }
        
        var date = DateOnly.ParseExact(sheet.Cells[_column, 2].GetValue<string>(),"dd/MM/yyyy");
        var store = sheet.Cells[_column, 3].GetValue<string>()
            .TrimStart()
            .TrimStart(StringComparison.OrdinalIgnoreCase, "VIPPS*", "ZETTLE_*", "SUMUP  *", "NYX*", "MS*", "KLARNA*")
            .Trim();
        var city = sheet.Cells[_column, 4].GetValue<string>();
        var currency = sheet.Cells[_column, 5].GetValue<string>();
        var amount = sheet.Cells[_column, 6].GetValue<decimal>();
        var isOriginalCurrency = currency.Equals("nok", StringComparison.OrdinalIgnoreCase);

        _expenses.Add(Expense.New(date, _user ?? throw new ArgumentException("No user in file"),store, city, currency, amount, trip: isOriginalCurrency ? null : "UNKNOWN"));
        _column++;
    }
    

    private void ReadUser()
    {
        _user = sheet.Cells[_column, 2].Text;
        _state = State.ReadingTransactions;
        _column += 3;
    }

    [GeneratedRegex(@"\d+\*+\d+")]
    private static partial Regex CreditCardNumber();
    private void WaitingForUser()
    {
        var cell = sheet.Cells[_column,1].Text;
        if (CreditCardNumber().IsMatch(cell))
        {
            _state = State.ReadingUser;
            return;
        }

        if (_expenses.Count != 0) 
            _state = State.EndOfFile;
        _column++;
    }
    
}