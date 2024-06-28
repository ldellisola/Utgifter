using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using NPOI.HSSF.UserModel;
using OfficeOpenXml;
using Utgifter.Api.Configuration;
using Utgifter.Api.Extensions;
using Utgifter.Api.Models;

namespace Utgifter.Api.Features.Expenses.Upload;

internal sealed class Endpoint(IOptions<DataBaseOptions> dbOptions) : Endpoint<Request, Response>
{
    private readonly string _connectionString = dbOptions.Value.ConnectionString;
    private const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string Xls = "application/vnd.ms-excel";
    
    public override void Configure()
    {
        Post("/expenses/upload");
        AllowFileUploads();
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        if (Files.Count == 0)
            AddError("No file uploaded");
        
        if (Files.Count > 1)
            AddError("Only one file can be uploaded at a time");
        
        if (req.ExpenseFile.ContentType != Xlsx && req.ExpenseFile.ContentType != Xls)
            AddError("Only Excel files are supported");
        
        ThrowIfAnyErrors();
        
        var expenses = await ParseExpensesAsync(req.ExpenseFile);
        
        var existingExpenses = new List<Expense>();
        var newExpenses = new List<Expense>();
        
        foreach (var expense in expenses)
        {
            // Check if expense already exists
            // if it does, add it to existingExpenses
            if (await GetExistingExpense(expense.Date, expense.Person, expense.Store, expense.Amount) is { } existingExpense)
            {
                existingExpenses.Add(existingExpense);
                continue;
            }
            
            // if it doesn't, add it to newExpenses
            var rule = await GetRule(expense.Store);
            var category = await GetCategory(rule?.NewStore ?? expense.Store);
            newExpenses.Add(expense with
            {
                Category = rule?.NewCategory ?? category,
                Store = rule?.NewStore ?? expense.Store,
                Shared = rule?.Shared ?? expense.Shared,
                Trip = rule?.Trip ?? expense.Trip
            });
        }
        
        await SendOkAsync(new(existingExpenses, newExpenses), ct);
    }
    

    private async Task<Rule?> GetRule(string store)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Rule>(
            $"""
             select id, expectedstore, newstore, newcategory, shared,trip
             from rules
             where (expectedstore NOT LIKE '*%' AND expectedstore NOT LIKE '%*' AND expectedstore = @store)
             OR 
             (expectedstore LIKE '%*' AND expectedstore NOT LIKE '*%' AND @store LIKE SUBSTRING(expectedstore FROM 1 FOR LENGTH(expectedstore)-1) || '%')
             OR
             (expectedstore LIKE '*%' AND expectedstore NOT LIKE '%*' AND @store LIKE '%' || SUBSTRING(expectedstore FROM 2))
             OR
             (expectedstore LIKE '*%' AND expectedstore LIKE '%*' AND @store LIKE '%' || SUBSTRING(expectedstore FROM 2 FOR LENGTH(expectedstore)-2) || '%')
             
             """,
            new {store}
        );
    }
    private async Task<string?> GetCategory(string store)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<string>(
            $"""
             select category from Expenses
             where store = @store
             """,
            new {store}
        );
    } 

    private async Task<Expense?> GetExistingExpense(DateOnly date, string person, string store, decimal amount)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var expense = await connection.QueryFirstOrDefaultAsync<Expense>(
            $"""
             select id, date, person, store, city, originalcurrency, amount, category, shared, trip 
             from Expenses
             where date = @date and person = @person and store = @store and amount = @amount
             """,
            new {date, person, store, amount}
        );

        return expense;
    }

    private static MemoryStream ConvertToXlsx(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var xlsWorkbook = new HSSFWorkbook(stream);
        var xlsx = new MemoryStream();
        xlsWorkbook.ToXlsx(xlsx);
        xlsx.Position = 0;
        return xlsx;
    }

    private static async Task<Expense[]> ParseExpensesAsync(IFormFile file)
    {
        await using var stream = file.ContentType == Xls ?
            ConvertToXlsx(file)
            : file.OpenReadStream();
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.First();

        var user = worksheet.Cells["C4"].GetValue<string>();

        var expenseIndex = 7;
        var expenses = new List<Expense>();
        while(worksheet.Cells[expenseIndex, 2].Value is not null) 
        {
            var date = DateOnly.FromDateTime(worksheet.Cells[expenseIndex, 1].GetValue<DateTime>());
            var store = worksheet.Cells[expenseIndex, 3].GetValue<string>();
            var city = worksheet.Cells[expenseIndex, 4].GetValue<string>();
            var originalCurrency = worksheet.Cells[expenseIndex, 5].GetValue<string?>();
            var amount = worksheet.Cells[expenseIndex, 7].GetValue<decimal>();
            
            expenses.Add(new(Guid.NewGuid(), date, user, store, city, originalCurrency ?? "NOK", amount,Trip: originalCurrency is not null));
            
            expenseIndex += originalCurrency is not null ? 2 : 1;
        }
        return expenses.ToArray();
    } 
}