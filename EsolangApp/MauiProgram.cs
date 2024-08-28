using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;

namespace EsolangApp;

static class MauiProgram {
	static MauiApp CreateMauiApp() {
		MauiAppBuilder b = MauiApp.CreateBuilder();
        
		b.UseMauiApp<App>().UseMauiCommunityToolkit()
                           .ConfigureFonts(fonts => {
                               fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                               fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                               
        }); return b.Build();
	}
}
