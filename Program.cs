using Microsoft.EntityFrameworkCore;
using ParallelEfContext.Context;
using ParallelEfContext.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<AppDbContext>(x =>
{
    x.UseSqlite(AppDbContextFactory.ConnectionString);
    
});

var app = builder.Build();

var dbConext = new AppDbContextFactory().CreateDbContext(new string[] { });
dbConext.Database.Migrate();

// Seed DB
dbConext.Letters.ExecuteDelete();
char letter = 'a';
while (letter != 'g')
{
    dbConext.Letters.Add(new Letter
    {
        Char = letter++,
    });
}
dbConext.SaveChanges();

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
