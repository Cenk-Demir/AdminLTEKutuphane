using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AdminLTEKutuphane.Services;

var builder = WebApplication.CreateBuilder(args);

// Firebase ayarlarını ve diğer servisleri yapılandırma
builder.Services.AddSingleton<FirestoreService>();

// MVC yapısını ekleyin
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware konfigürasyonları
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
