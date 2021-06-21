using System;
using System.Reflection;
using System.Text.Json.Serialization;
using DesafioInoa.Api.Services;
using DesafioInoa.App;
using DesafioInoa.App.Services;
using DesafioInoa.Domain.Handlers;
using DesafioInoa.Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DesafioInoa.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSingleton<TokenStorageService, TokenStorageService>();
            services.AddScoped<IMarketDataService, HGFinanceService>();
            services.AddScoped<IMailService, MailSmtpService>();
            services.AddTransient<StockHandler, StockHandler>();
            services.AddTransient<StockQuoteAlert, StockQuoteAlert>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                // Use method name as operationId
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DesafioInoa.Api",
                    Version = "v1",
                    Description = "API direcionada ao monitoramento de ações da B3.",
                    TermsOfService = new Uri("https://www.inoa.com.br/"),
                    Contact = new OpenApiContact
                    {
                        Name = "Fábio Trevizolo de Souza",
                        Email = "fabiots@inoa.com.br",
                        Url = new Uri("https://github.com/FabioTS"),
                    }
                    ,
                    License = new OpenApiLicense
                    {
                        Name = "Inoa.licx",
                        Url = new Uri("https://www.inoa.com.br/"),
                    }
                });
                c.IncludeXmlComments(GetXmlPath());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DesafioInoa.Api v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string GetXmlPath()
        {
            string apiName = Assembly.GetExecutingAssembly().GetName().Name;
            string xmlFile = apiName + ".xml";
            try
            {
                string slnPath = System.IO.Directory.GetParent(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf(apiName))).ToString();
                string apiPath = System.IO.Path.Combine(slnPath, apiName);
                return System.IO.Path.Combine(apiPath + "/bin", xmlFile);
            }
            catch
            {
                return System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            }
        }
    }
}
