using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Postamat.DataBases;
using Postamat.Models;
using Postamat.Models.Mapping;
using Postamat.Repositories;
using Postamat.Services;

namespace Postamat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // добавляем контекст ApplicationContext
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ApplicationConnection")));
            // добавляем контекст IdentityContext
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));
            services.AddMvc();
            
            // добавляеи репотзитории
            services.AddTransient<IOrderPriceCalculator, OrderPriceCalculator>();
            services.AddTransient<ITracking, Tracking>();
            services.AddTransient<IRepository<Models.Postamat>, ApplicationRepository<Models.Postamat>>();
            services.AddTransient<IRepository<Order>, ApplicationRepository<Order>>();
            services.AddTransient<IRepository<Product>, ApplicationRepository<Product>>();
            services.AddTransient<IRepository<CartLine>, ApplicationRepository<CartLine>>();
            services.AddTransient<IRepository<Customer>, ApplicationRepository<Customer>>();
            services.AddTransient<IRepository<User>, IdentityRepository<User>>();

            // добавляем маппер
            var mapperConfig = new MapperConfiguration(mc => mc.AddProfile(new MappingProfiler()));
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // конфигурация авторизации на основе JWT токенов
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = AuthOptions.ISSUER,
                            ValidateAudience = true,
                            ValidAudience = AuthOptions.AUDIENCE,
                            ValidateLifetime = true,
                            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                            ValidateIssuerSigningKey = true,
                        };
                    });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseDefaultFiles();
            
            // разрешаем использование статичных файлов для отбражения представления из wwwroot
            app.UseStaticFiles();

            // встраиваем в конвейер компонент AuthenticationMiddleware, который управляет аутентификацией
            app.UseAuthentication();

            // встраиваем в конвейер компонент AuthorizationMiddleware, который управляет авторизацией пользователей и разграничивает доступ к ресурсам
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // заполняем БД
            ContextSeedData.EnsurePopulated(app);
        }
    }
}