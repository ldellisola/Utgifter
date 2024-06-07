using Microsoft.Extensions.Options;

namespace Utgifter.Api.Configuration;

public class DataBaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class DataBaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DataBaseOptions>
{
    public void Configure(DataBaseOptions options)
    {
        options.ConnectionString = configuration.GetConnectionString("postgres") ?? throw new ArgumentException("Invalid connection string");
    }
}