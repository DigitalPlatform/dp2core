using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dp2LibraryServer
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
            // session support
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-5.0
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllers()
                // https://stackoverflow.com/questions/36452468/swagger-ui-web-api-documentation-present-enums-as-strings
                .AddJsonOptions(x =>
                {
                    // 禁止返回的属性名使用 camel 形态
                    // https://stackoverflow.com/questions/58476681/asp-net-core-3-0-system-text-json-camel-case-serialization
                    x.JsonSerializerOptions.PropertyNamingPolicy = null;
                    x.JsonSerializerOptions.WriteIndented = true;
                    // x.JsonSerializerOptions.Converters.Add(new Controllers.v3Controller.ByteArrayConverter());
                });

            services.AddSwaggerGen(c =>
            {
                // https://stackoverflow.com/questions/58834430/c-sharp-net-core-swagger-trying-to-use-multiple-api-versions-but-all-end-point
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Title = "dp2LibraryServer API V3",
                    Description = "dp2 图书馆集成系统应用服务器 API",
                    Version = "v3",
                    License = new OpenApiLicense
                    {
                        Name = "Apache-2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "xietao",
                        Email = "xietao@dp2003.com",
                        Url = new Uri("https://github.com/DigitalPlatform/dp2core")
                    }
                });

                c.SwaggerDoc("v4", new OpenApiInfo
                {
                    Title = "dp2LibraryServer API V4",
                    Description = "dp2 图书馆集成系统应用服务器 API",
                    Version = "v4",
                    License = new OpenApiLicense
                    {
                        Name = "Apache-2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "xietao",
                        Email = "xietao@dp2003.com",
                        Url = new Uri("https://github.com/DigitalPlatform/dp2core")
                    }
                });

                // c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                // version < 3.0 like this: c.OperationFilter<ExamplesOperationFilter>(); 
                // version 3.0 like this: c.AddSwaggerExamples(services.BuildServiceProvider());
                // version > 4.0 like this:
                c.ExampleFilters();
                // c.SchemaFilter<EnumSchemaFilter>();

            });

            services.AddSwaggerExamplesFromAssemblyOf<Controllers.v3Controller.LoginResponseExamples>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/v3/swagger.json", "dp2LibraryServer API V3");
                    c.SwaggerEndpoint($"/swagger/v4/swagger.json", "dp2LibraryServer API V4");
                });
                // app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "dp2LibraryServer v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            // session support
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
