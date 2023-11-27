using MailboxApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load domains.json
var filePath = builder.Environment.ContentRootPath + "\\Data\\Static";
var domainsConfig = new ConfigurationBuilder()
    .SetBasePath(filePath)
    .AddJsonFile("Domains.json", optional: false, reloadOnChange: true)
    .Build();

// Add services to the container.
builder.Services.ProjectSettings(builder.Configuration, domainsConfig);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
