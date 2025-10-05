// Here you could define global logic that would affect all tests

// You can use attributes at the assembly level to apply to all tests in the assembly

using OfficeOpenXml;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace Utgifter.Tests;

public class GlobalHooks
{
    [Before(TestSession)]
    public static void SetUp()
    {
        ExcelPackage.License.SetNonCommercialPersonal("lucas");
    }
}
