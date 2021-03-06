using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestWebApiServer
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
                options.IdleTimeout = TimeSpan.FromSeconds(10);
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
                    // x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestWebApiServer", Version = "v1" });

                // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, @"MyApi.xml"));
                // c.UseInlineDefinitionsForEnums();

                // version < 3.0 like this: c.OperationFilter<ExamplesOperationFilter>(); 
                // version 3.0 like this: c.AddSwaggerExamples(services.BuildServiceProvider());
                // version > 4.0 like this:
                c.ExampleFilters();
                c.SchemaFilter<EnumSchemaFilter>();

            });



            // https://stackoverflow.com/questions/36452468/swagger-ui-web-api-documentation-present-enums-as-strings
            // services.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()

            services.AddSwaggerExamplesFromAssemblyOf<Controllers.LoginResponseExamples>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                /*
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(c =>
                {
                    c.SerializeAsV2 = true;
                });
                */
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestWebApiServer v1"));
            }


            app.UseRouting();

            // app.UseAuthorization();

            // session support
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
