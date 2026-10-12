using ApplicationServices.Services;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAutentication(builder.Configuration);
builder.Services.AddContexts(builder.Configuration);


builder.Services.AddCors(o => o.AddPolicy("AllowFront", p =>
    p.WithOrigins("http://localhost:58433")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

builder.Services.AddRepositories();
builder.Services.AddControllers();
builder.Services.AddBus(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.AddSwaggerUI();

app.UseCors("AllowFront");
app.AddErrorMiddleware();
app.UseHttpsRedirection();
app.AddMetrics();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
