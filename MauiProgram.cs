using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Vocon.Services.EmbeddingServices;
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
