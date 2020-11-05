using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleWebProxy.Codes;

namespace SimpleWebProxy
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
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/SetHost", async context =>
                {
                    var host = context.Request.Query["host"].ToString();
                    if (string.IsNullOrWhiteSpace(host))
                    {
                        await context.Response.WriteAsync("SetHost Error! Parameter host is null!");
                    }
                    host = host.Replace("https://", "").Replace("http://", "").Replace(" ", "").Trim().Trim('/');
                    context.Response.Cookies.Append("host", host);
                    await context.Response.WriteAsync($"SetHost {host}!");
                });
                endpoints.MapGet("/AddHost", async context =>
                {
                    var host = context.Request.Query["host"].ToString();
                    if (string.IsNullOrWhiteSpace(host))
                    {
                        await context.Response.WriteAsync("AddHost Error! Parameter host is null!");
                    }
                    var name = context.Request.Query["name"].ToString();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await context.Response.WriteAsync("AddHost Error! Parameter name is null!");
                    }
                    host = host.Replace("https://", "").Replace("http://", "").Replace(" ", "").Trim().Trim('/');
                    name = name.Replace("https://", "").Replace("http://", "").Replace(" ", "").Trim().Trim('/');
                    context.Response.Cookies.Append("host-" + name, host);
                    await context.Response.WriteAsync($"AddHost {name}: {host}");
                });
                endpoints.MapGet("/ClearHost", async context =>
                {
                    var cookies = context.Request.Cookies;
                    foreach (var item in cookies)
                    {
                        context.Response.Cookies.Delete(item.Key);
                    }
                    await context.Response.WriteAsync("ClearHost OK!");
                });
                endpoints.MapGet("{*url}", async context =>
                {
                    if (DownloadHelper.HasCookie(context))
                    {
                        await DownloadHelper.Download(context);
                        return;
                    }
                    await context.Response.WriteAsync("Error!");
                });
                endpoints.MapPost("{*url}", async context =>
                {
                    if (DownloadHelper.HasCookie(context))
                    {
                        await DownloadHelper.Post(context);
                        return;
                    }
                    await context.Response.WriteAsync("Error!");
                });

            });
        }
    }
}
