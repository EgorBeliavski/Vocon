using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.Audio;
using Vocon.Pages;
using Vocon.Services;
using Vocon.Services.AutoStartService;
using Vocon.Services.BrowserNavigationService;
using Vocon.Services.CommandService;
using Vocon.Services.EmbeddingServices;
using Vocon.Services.HotKeyService;
using Vocon.Services.MicroDeviceService;
using Vocon.Services.SettingLanguageService;
using Vocon.Services.WhisperService;
using Vocon.TagSercices;
using Vocon.ViewModels;
using Microsoft.UI;
using Vocon.Platforms.Windows;

#if WINDOWS
using Microsoft.UI.Windowing;
using WinRT.Interop;
#endif

namespace Vocon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<SettingsPageViewModel>();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows => windows.OnWindowCreated(window =>
                    {
                        var hwnd = WindowNative.GetWindowHandle(window);
                        var id = Win32Interop.GetWindowIdFromWindow(hwnd);
                        var appWindow = AppWindow.GetFromWindowId(id);

                        if (appWindow.Presenter is OverlappedPresenter presenter)
                        {
                            presenter.SetBorderAndTitleBar(hasBorder: false, hasTitleBar: false);
                        }

                        BorderlessWindowHelper.RemoveHairlineBorder(hwnd);

                        var chrome = IPlatformApplication.Current!.Services
                            .GetRequiredService<WindowChromeService>();
                        chrome.Attach(window);
                    }));
#endif
                });

            builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);
            builder.Services.AddSingleton<WhisperService>();
            builder.Services.AddSingleton<EmbeddingService>();
            builder.Services.AddSingleton<TagService>();
            builder.Services.AddSingleton<CommandService>();
            builder.Services.AddSingleton<WindowChromeService>();

            builder.Services.AddSingleton<ISettingLanguageService, SettingLanguageService>();
            builder.Services.AddSingleton<IMediaControlService, MediaControlService>();
            builder.Services.AddSingleton<IHotKeySettingsService, HotKeySettingsService>();
            builder.Services.AddSingleton<IHotKeyService, HotKeyService>();
            builder.Services.AddSingleton<IHotKeyRecorderService, HotKeyRecorderService>();
            builder.Services.AddSingleton<INoteRepository, NoteRepository>();
            builder.Services.AddSingleton<IMicrophoneSettingsService, MicrophoneSettingsService>();
            builder.Services.AddSingleton<IBrowserNavigationService, BrowserNavigationService>();

            builder.Services.AddSingleton<MicroDeviceService>();
            builder.Logging.AddDebug();
            builder.Services.AddSingleton<AutoStartService>();

            var app = builder.Build();

            Task.Run(async () =>
            {
                await app.Services.GetRequiredService<INoteRepository>().InitializeAsync();
            }).GetAwaiter().GetResult();

            app.Services.GetRequiredService<EmbeddingService>()
                   .InitializeAsync()
                   .GetAwaiter()
                   .GetResult();

            app.Services.GetRequiredService<TagService>().Initialize();
            app.Services.GetRequiredService<CommandService>().Initialize();

            return app;
        }
    }
}