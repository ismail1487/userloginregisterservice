using AutoMapper;
using Baz.AOP.Logger.ExceptionLog;
using Baz.AOP.Logger.Http;
using Baz.Model.Entity;
using Baz.Model.Entity.ViewModel;
using Baz.Model.Pattern;
using Baz.RequestManager;
using Baz.RequestManager.Abstracts;
using Baz.UserLoginServiceApi;
using Baz.SharedSession;
using Baz.UserLoginServiceApi.Handlers;
using Baz.UserLoginServiceApi.Helper;
using BazWebApp.Services;
using Decor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Baz.Service;
using Baz.AletKutusu;
using Baz.Model.Entity.Constants;
using static System.Collections.Specialized.BitVector32;
using System.Configuration;

namespace Baz.UserLoginServiceApi
{
    /// <summary>
    /// Uygulama ayağa kaldırılırken ilk çalışan sınıf
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Uygulama ayağa kaldırılırken ilk çalışan sınıf konst.
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables().Build();
        }

        /// <summary>
        /// Api ayarlarının yapıldığı metod
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Bu yöntem çalışma zamanı tarafından çağrılır. Kapsayıcıya hizmet eklemek için bu yöntemi kullanın.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            Model.Entity.Constants.LocalPortlar.CoreUrl = Configuration.GetValue<string>("CoreUrl");

            var section = Configuration.GetSection("LocalPortlar");
            LocalPortlar.WebApp = section.GetValue<string>("WebApp");
            LocalPortlar.UserLoginregisterService = section.GetValue<string>("UserLoginregisterService");
            LocalPortlar.KisiServis = section.GetValue<string>("KisiServis");
            LocalPortlar.MedyaKutuphanesiService = section.GetValue<string>("MedyaKutuphanesiService");
            LocalPortlar.IYSService = section.GetValue<string>("IYSService");
            LocalPortlar.KurumService = section.GetValue<string>("KurumService");

            services.AddHttpContextAccessor();
            services.AddControllers(c => { c.Filters.Add(typeof(ModelValidationFilter), int.MinValue); });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Baz.UserLoginServiceApi", Version = "v1" });
                c.OperationFilter<DefaultHeaderParameter>();
            });

            //////////////////////////////////////////SESSION SERVER AYARLARI/////////////////////////////////////////////////
            //Distributed session i�lemleri i�in session server�n network ba�lant�lar�n� yap�land�r�r.
            services.AddDistributedSqlServerCache(p =>
            {
                p.ConnectionString = Configuration.GetConnectionString("SessionConnection");
                p.SchemaName = "dbo";
                p.TableName = "SQLSessions";
            });
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Path = "/";
                options.Cookie.Name = "Test.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(60);
            });
            services.AddSession();
            //Http deste�i olmadan payla��ml� session i�lemleri yapan servisi kay�t eder.
            services.AddTransient<Baz.SharedSession.ISharedSession, Baz.SharedSession.BaseSharedSession>();
            //Http deste�i olan i�lemler i�in payla��ml� session nesnesinin kayd�n� yapar.
            //BaseSharedSessionForHttpRequest i�lemleri i�in �ncelikle BaseSharedSession servisi kay�t edilmelidir.
            services.AddTransient<Baz.SharedSession.ISharedSessionForHttpRequest, Baz.SharedSession.BaseSharedSessionForHttpRequest>();
            //////////////////////////////////////////////////////////////////////////////////////

            services.AddDbContext<Repository.Pattern.IDataContext, Repository.Pattern.Entity.DataContext>(conf => conf.UseSqlServer(Configuration.GetConnectionString("Connection")));
            services.AddSingleton<Baz.Mapper.Pattern.IDataMapper>(new Baz.Mapper.Pattern.Entity.DataMapper(GenerateConfiguratedMapper()));
            services.AddScoped<Repository.Pattern.IUnitOfWork, Repository.Pattern.Entity.UnitOfWork>();
            services.AddScoped(typeof(Repository.Pattern.IRepository<>), typeof(Repository.Pattern.Entity.Repository<>));
            services.AddScoped(typeof(Service.Base.IService<>), typeof(Service.Base.Service<>));
            services.AddScoped<ISistemSayfalariService, SistemSayfalariService>();
            services.AddScoped<ISistemLoginTarihceService, SistemLoginTarihceService>();
            services.AddScoped<ISistemLoginSonDurumService, SistemLoginSonDurumService>();
            services.AddScoped<ISistemLoginSifreYenilemeAktivasyonHareketleriService, SistemLoginSifreYenilemeAktivasyonHareketleriService>();
            services.AddScoped<ISistemLoginCariDurumService, SistemLoginCariDurumService>();
            services.AddScoped<ISayfaYetkiGurubuAyrintilarService, SayfaYetkiGurubuAyrintilarService>();
            services.AddScoped<IPostaciBekleyenIslemlerGenelService, PostaciBekleyenIslemlerGenelService>();
            services.AddScoped<IPostaciBekleyenIslemlerAyrintilarService, PostaciBekleyenIslemlerAyrintilarService>();
            services.AddScoped<IParamOrganizasyonBirimleriService, ParamOrganizasyonBirimleriService>();
            services.AddScoped<IParamKureselParametrelerService, ParamKureselParametrelerService>();
            services.AddScoped<IModulService, ModulService>();
            services.AddScoped<IModulDetayService, ModulDetayService>();
            services.AddScoped<IKurumTemelBilgilerService, KurumTemelBilgilerService>();
            services.AddScoped<IKurumOrganizasyonBirimTanimlariService, KurumOrganizasyonBirimTanimlariService>();
            services.AddScoped<IKurumlarKisilerService, KurumlarKisilerService>();
            services.AddScoped<IKurumIliskiService, KurumIliskiService>();
            services.AddScoped<IKisiTemelBilgilerService, KisiTemelBilgilerService>();
            services.AddScoped<IKisiService, KisiService>();
            services.AddScoped<IKisiIliskiService, KisiIliskiService>();
            services.AddScoped<IKisiHassasBilgilerService, KisiHassasBilgilerService>();
            services.AddScoped<IErisimYetkilendirmeTanimlariService, ErisimYetkilendirmeTanimlariService>();

            services.AddScoped<IYetkiMerkeziService, YetkiMerkeziService>();
            services.AddTransient<ILoginUser, LoginUserManager>();
            services.AddTransient<IRequestHelper, RequestHelper>(provider =>
            {
                return new RequestHelper("", new RequestManagerHeaderHelperForHttp(provider).SetDefaultHeader());
            });

            services.AddSingleton<IRequestHelper, RequestHelper>();
            services.AddSingleton<IKureselParametrelerService, KureselParametrelerService>();

            //Exception loglar�n� i�leyen Baz.AOP.Logger.ExceptionLog servisinin kayd�n� yapar
            //services.AddAOPExceptionLogging();
            //Http i�lemleri i�in loglama yapan BaseHttpLogger servisinin kayd�n� yapar.

            var types = typeof(Service.Base.IService<>).Assembly.GetTypes();
            var interfaces = types.Where(p => p.IsInterface && p.GetInterface("IService`1") != null).ToList();

            foreach (var item in interfaces)
            {
                var serviceTypes = types.Where(p => p.GetInterface(item.Name) != null && !p.IsInterface).ToList();
                serviceTypes.ForEach(p => services.AddScoped(item, p));
            }
            services.AddResponseCompression();
            //BaseHttpLogger nesnesini b�t�n controllerlar i�in filter servisi olarak tan�mlan�r.
            services.AddControllers();
        }

        /// <summary>
        /// Bu yöntem çalışma zamanı tarafından çağrılır. HTTP istek ardışık düzenini yapılandırmak için bu yöntemi kullanın.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="lifetime"></param>
        /// <param name="cache"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
            IDistributedCache cache)
        {

            app.UseRequestLocalization();
            ////////////////////////////////// SESSION SERVER AYARLARI/////////////////////////////////////
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseSession();
            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUTC = DateTime.UtcNow.ToString();
                byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20));
                cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            });
            /////////////////////////////////////////////////////////////////////////////////////

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "Baz.UserLoginServiceApi v1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseMiddleware<AuthMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Mapper ayarlarının yapıldığı metot.
        /// </summary>
        /// <returns></returns>
        private Profile GenerateConfiguratedMapper()
        {
            var mapper = Baz.Mapper.Pattern.Entity.DataMapperProfile.GenerateProfile();
            mapper.CreateMap<KisiTemelBilgiler, KullaniciSession>();
            mapper.CreateMap<KullaniciSession, KisiTemelBilgiler>();
            return mapper;
        }
    }
}