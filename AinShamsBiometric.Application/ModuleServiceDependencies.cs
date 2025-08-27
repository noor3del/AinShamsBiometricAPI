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
            services.AddScoped<IICAOService, ICAOService>();
            return services;
        }
    }
}
