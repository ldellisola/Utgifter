using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Npgsql;
using NPOI.HSSF.UserModel;
using OfficeOpenXml;
using Utgifter.Api.Configuration;
using Utgifter.Api.Extensions;
using Utgifter.Api.Features.Expenses.Upload.ExcelParsers;
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
        
        if (req.ExpenseFile.ContentType != Xlsx)
            AddError("Only XLSX files are supported");
        
        ThrowIfAnyErrors();
        
        var expenses = await ParseExpensesAsync(req.ExpenseFile);
        
        var existingExpenses = new List<Expense>();
        var newExpenses =  new List<Expense>();
        
        foreach (var expense in expenses)
        {
            // Check if expense already exists
            // if it does, add it to existingExpenses
            if (await GetExistingExpense(expense) is { } existingExpense)
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
                Trip = rule?.Trip switch
                {
                    false => null,
                    true => "UNKNOWN",
                    null => null,
                }
            });
        }
        
        await Send.OkAsync(new(existingExpenses, newExpenses), ct);
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

    private async Task<Expense?> GetExistingExpense(Expense e)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var expense = await connection.QueryFirstOrDefaultAsync<Expense>(
            $"""
             select id, date, person, store, city, originalcurrency, amount,hash, category, shared, trip 
             from Expenses
             where expenses.hash = @hash
             """,
            new {hash = e.Hash}
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
        await using var stream = file.ContentType switch
        {
            Xlsx => file.OpenReadStream(),
            _ => throw new NotSupportedException("Unsupported file type")
        };
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.First();

        return worksheet.Cells[1, 1].GetValue<string>().ToLowerInvariant() switch
        {
            "fakturadetaljer" => new FakturaReportParser(worksheet).Parse(),
            "transaksjonseksport" => new TransactionListParser(worksheet).Parse()
        };
    }
}