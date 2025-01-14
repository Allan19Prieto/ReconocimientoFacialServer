using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ReconocimientoFacialServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddScoped<FaceRecognizerService>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // Permitir hasta 10 MB
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Tiempo de espera del cliente
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);      // Tiempo de espera para el handshake inicial
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);     // Mantener la conexión activa
    });
builder.Services.AddSingleton<ReconocimientoFacialServer.Data.DatabaseHandler>();
builder.Services.AddSingleton<ReconocimientoFacialServer.Services.FaceRecognizerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
