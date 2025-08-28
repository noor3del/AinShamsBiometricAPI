using AinShamsBiometric.Application.Interfaces;
using AinShamsBiometric.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Neurotec.Biometrics.Client;

namespace AinShamsBiometric.Application
{
    public static class ModuleServiceDependencies
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
        {
            services.AddSingleton<NBiometricClient>(sp =>
            {
                var client = new NBiometricClient();
                return client;
            });
            services.AddScoped<IFaceService, FaceService>();
            services.AddHttpClient<ICardService, CardService>(client =>
            {
                client.BaseAddress = new Uri("https://api.miniai.live/"); // trailing slash required
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            });



            return services;
        }
    }
}
