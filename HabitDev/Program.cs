using HabitDev;
using HabitDev.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{ options.ReturnHttpNotAcceptable = true; })
.AddNewtonsoftJson()
.AddXmlSerializerFormatters();

builder.Services.AddOpenApi();
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();


app.MapControllers();

await app.RunAsync();
