using AccountServer.DB;
using Microsoft.EntityFrameworkCore;
using SharedDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = null; //json으로 만들때 프로퍼티네임 그대로 Json스트링으로 만들어줌.
		options.JsonSerializerOptions.DictionaryKeyPolicy = null;
	});

builder.Services.AddDbContext<AppDbContext>(
	options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<SharedDbContext>(
	options => options.UseSqlServer(builder.Configuration.GetConnectionString("SharedDBConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
