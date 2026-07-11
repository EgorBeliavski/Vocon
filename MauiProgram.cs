using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Vocon.Pages;
using Vocon.Services.EmbeddingServices;
using Vocon.Services.HotKeyService;
using Vocon.Services.MicroDeviceService;
using Vocon.Services.SettingLanguageService;
using Vocon.Services.WhisperService;
using Vocon.TagSercices;
using Vocon.ViewModels;

namespace Vocon
{
    public static class MauiProgram
    {
        public  static MauiApp CreateMauiApp()
        {
           

           

        
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsPageViewModel>();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }); 
            builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);
            builder.Services.AddSingleton<WhisperService>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddSingleton<EmbeddingService>();
            builder.Services.AddSingleton<TagService>();
            builder.Services.AddSingleton<ISettingLanguageService, SettingLanguageService>();
            builder.Services.AddSingleton<MicroDeviceService>();
            builder.Services.AddSingleton<HotKeyService>();
            builder.Logging.AddDebug();

            var app = builder.Build();

            app.Services.GetRequiredService<EmbeddingService>()
                   .InitializeAsync()
                   .GetAwaiter()
                   .GetResult();
            app.Services.GetRequiredService<TagService>()
                    .Initialize();
#if DEBUG

#endif

            return app;
        }
    }
}
