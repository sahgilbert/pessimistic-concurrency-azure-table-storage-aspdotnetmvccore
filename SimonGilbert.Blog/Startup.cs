using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using SimonGilbert.Blog.Services;
using SimonGilbert.Blog.Repositories;

namespace SimonGilbert.Blog
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }
        private static string AzureTableStorageDebugConnectionString { get; set; }
        private const string OrdersCloudTableName = "Orders";
        private const string OrdersCloudBlobContainerName = "orders";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Create an Autofac Container and push the framework services
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            // Register services and repositories within Autofac
            ConfigureStorageAccount(containerBuilder);
            ConfigureServicesWithRepositories(containerBuilder);
            ConfigureAzureCloudTables(containerBuilder);
            ConfigureAzureCloudBlobContainers(containerBuilder);
            ConfigureCloudBlobContainersForServices(containerBuilder);
            ConfigureCloudTablesForRepositories(containerBuilder);

            // Build the container and return an IServiceProvider from Autofac
            var container = containerBuilder.Build();
            return container.Resolve<IServiceProvider>();
        }

        private static void ConfigureStorageAccount(ContainerBuilder builder)
        {
            AzureTableStorageDebugConnectionString = Configuration["Azure:Storage:ConnectionString"];

            builder.Register(c => CreateStorageAccount(AzureTableStorageDebugConnectionString));
        }

        private static CloudStorageAccount CreateStorageAccount(string connection)
        {
            if (String.IsNullOrEmpty(connection))
            {
                throw new Exception("Azure Storage connection string is null!");
            }
            return CloudStorageAccount.Parse(connection);
        }

        private static void ConfigureServicesWithRepositories(ContainerBuilder builder)
        {
            builder.RegisterType<OrderService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }

        private static void ConfigureAzureCloudTables(ContainerBuilder builder)
        {
            builder.Register(c => c.Resolve<CloudStorageAccount>().CreateCloudTableClient());

            builder.Register(c => GetTable(c, OrdersCloudTableName))
                .Named<CloudTable>(OrdersCloudTableName);
        }

        private static CloudTable GetTable(IComponentContext context, string tableName)
        {
            var table = context.Resolve<CloudTableClient>().GetTableReference(tableName);

            var createdSuccessfully = table.CreateIfNotExistsAsync().Result;

            return table;
        }

        private static void ConfigureAzureCloudBlobContainers(ContainerBuilder builder)
        {
            builder.Register(c => c.Resolve<CloudStorageAccount>().CreateCloudBlobClient());

            builder.Register(c => GetBlobContainer(c, OrdersCloudBlobContainerName))
                .Named<CloudBlobContainer>(OrdersCloudBlobContainerName);
        }

        private static CloudBlobContainer GetBlobContainer(IComponentContext context, string blobContainerName)
        {
            var blob = context.Resolve<CloudBlobClient>().GetContainerReference(blobContainerName);

            var createdSuccessfully = blob.CreateIfNotExistsAsync().Result;

            if (createdSuccessfully)
            {
                blob.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            }

            return blob;
        }

        private static void ConfigureCloudBlobContainersForServices(ContainerBuilder builder)
        {
            builder.RegisterType<OrderService>()
                   .WithParameter(
                       (pi, c) => pi.ParameterType == (typeof(CloudBlobContainer)),
                       (pi, c) => c.ResolveNamed<CloudBlobContainer>(OrdersCloudBlobContainerName))
                       .AsImplementedInterfaces();
        }

        private static void ConfigureCloudTablesForRepositories(ContainerBuilder builder)
        {
            builder.RegisterType<OrderRepository>()
                   .WithParameter(
                       (pi, c) => pi.ParameterType == (typeof(CloudTable)),
                       (pi, c) => c.ResolveNamed<CloudTable>(OrdersCloudTableName))
                       .AsImplementedInterfaces();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}