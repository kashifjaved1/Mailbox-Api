using MailboxApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MailboxApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ProjectSettings(this IServiceCollection services, IConfiguration configuration, IConfigurationRoot configOptions)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddDbContext<ApiDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("Default")));

            services.Configure<DomainsOptions>(configOptions);
        }
    }
}
