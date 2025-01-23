﻿using Blazored.Toast.Services;
using IPrint.Services;
using Microsoft.Extensions.Logging;
using AlertService = IPrint.Services.AlertService;

namespace IPrint
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            //Services
            builder.Services.AddScoped<PrinterService>();
            builder.Services.AddScoped<PrintSpoolerService>();
            builder.Services.AddScoped<UserConfigService>();
            builder.Services.AddScoped<PrinterConfigService>();
            builder.Services.AddScoped<LabelProfileService>();
            builder.Services.AddScoped<FileStorageService>();
            builder.Services.AddScoped<AlertService>();
            builder.Services.AddScoped<ToastService>();
            builder.Services.AddScoped<PrintSpoolerService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
