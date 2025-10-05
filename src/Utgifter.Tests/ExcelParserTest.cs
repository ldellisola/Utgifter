using OfficeOpenXml;
using Utgifter.Api.Features.Expenses.Upload;
using Utgifter.Api.Models;

namespace Utgifter.Tests;

public class ExcelParserTest
{
    [Test]
    public async Task ParseExcelFile()
    {
        // Arrange
        var fileName = "data/transactions.xlsx";
        await using var file = File.OpenRead(fileName);
        using var package = new ExcelPackage(file);
        var worksheet = package.Workbook.Worksheets.First();
        var parser = new ExcelParser(worksheet);
        
        // Test
        var expenses = parser.Parse();
        
        // Assert
        Expense[] expectedExpenses =
        [
            Expense.New(new DateOnly(2025,09,29), "Person 1","H&M","TORP","NOK",100),
            Expense.New(new DateOnly(2025,09,15), "Person 1","SAS","PRISHTINA","EUR",2000, trip: "UNKNOWN"),
            Expense.New(new DateOnly(2025,09,15), "Person 1","Store","SKOPJE","MKD",50, trip: "UNKNOWN"),
            Expense.New(new DateOnly(2025,09,15), "Person 1","REMA1000","SKOPJE","MKD",30, trip: "UNKNOWN"),
            Expense.New(new DateOnly(2025,09,10), "Person 1","KIWI","LYSAKER","NOK",300),
            Expense.New(new DateOnly(2025,09,10), "Person 1","JONK","OSLO","NOK",4000),
            Expense.New(new DateOnly(2025,09,30), "Person 2","Max Burgers","OSLO","NOK",90),
            Expense.New(new DateOnly(2025,09,29), "Person 2","Food AS","RAKKESTAD","NOK",new decimal(125.88)),
            Expense.New(new DateOnly(2025,09,22), "Person 2","Airport","CIDADE","EUR",600, trip: "UNKNOWN"),
            Expense.New(new DateOnly(2025,09,22), "Person 2","ESim","BUDAPEST","NOK",321),
            Expense.New(new DateOnly(2025,09,22), "Person 2","Ruter","OSLO","NOK",9090),
            Expense.New(new DateOnly(2025,09,22), "Person 2","Coffee","SKOPJE","MKD",233, trip: "UNKNOWN"),
        ];

        await Assert.That(expenses.Length).IsEqualTo(expectedExpenses.Length);

        foreach (var (expense, expected) in expenses.Zip(expectedExpenses))
        {
            await Assert.That(expense.Amount).IsEqualTo(expected.Amount).Because("Amounts must be equal");
            await Assert.That(expense.Date).IsEqualTo(expected.Date).Because("Date must be equal");
            await Assert.That(expense.City).IsEqualTo(expected.City).Because("City must be equal");
            await Assert.That(expense.OriginalCurrency).IsEqualTo(expected.OriginalCurrency)
                .Because("OriginalCurrency must be equal");
            await Assert.That(expense.Person).IsEqualTo(expected.Person).Because("Person must be equal");
            await Assert.That(expense.Store).IsEqualTo(expected.Store).Because("Store must be equal");
            await Assert.That(expense.Trip).IsEqualTo(expected.Trip).Because("Trip must be equal");
        }
    }
}