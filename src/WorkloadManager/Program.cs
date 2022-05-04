﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.WorkloadManager.Common;
using Monai.Deploy.WorkloadManager.Configuration;
using Monai.Deploy.WorkloadManager.Services.DataRetentionService;
using Monai.Deploy.WorkloadManager.Services.Http;
using Monai.Deploy.WorkloadManager.PayloadListener.Services;
using Monai.Deploy.WorkloadManager.PayloadListener.Validators;
using Monai.Deploy.Messaging.RabbitMq;
using Monai.Deploy.Messaging;
using Monai.Deploy.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Monai.Deploy.WorkloadManager.Common.Services;
using Monai.Deploy.WorkloadManager.Common.Services.Interfaces;
using Monai.Deploy.WorkloadManager.Database;
using Monai.Deploy.WorkloadManager.Database.Interfaces;
using Monai.Deploy.WorkloadManager.Database.Options;
using MongoDB.Driver;

namespace Monai.Deploy.WorkloadManager
{
    internal class Program
    {
        protected Program()
        { }

        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;
                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((builderContext, configureLogging) =>
                {
                    configureLogging.AddConfiguration(builderContext.Configuration.GetSection("Logging"));
                    configureLogging.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<WorkloadManagerOptions>()
                        .Bind(hostContext.Configuration.GetSection("WorkloadManager"))
                        .PostConfigure(options =>
                        {
                        });
                    services.AddOptions<MessageBrokerServiceConfiguration>()
                        .Bind(hostContext.Configuration.GetSection("messageConnection"))
                        .PostConfigure(options =>
                        {
                        });
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<WorkloadManagerOptions>, ConfigurationValidator>());

                    services.AddSingleton<ConfigurationValidator>();

                    services.AddSingleton<DataRetentionService>();

                    services.AddHostedService<DataRetentionService>(p => p.GetService<DataRetentionService>());

                    // Services
                    services.AddTransient<IWorkflowService, WorkflowService>();

                    services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

                    // Mongo DB
                    services.Configure<WorkloadManagerDatabaseSettings>(hostContext.Configuration.GetSection("WorkloadManagerDatabase"));
                    services.AddSingleton<IMongoClient, MongoClient>(s => new MongoClient(hostContext.Configuration["WorkloadManagerDatabase:ConnectionString"]));
                    services.AddTransient<IWorkflowRepository, WorkflowRepository>();
                    services.AddTransient<IWorkflowInstanceRepository, WorkflowInstanceRepository>();

                    // MessageBroker
                    services.AddSingleton<RabbitMqMessagePublisherService>();
                    services.AddSingleton<IMessageBrokerPublisherService>(implementationFactory =>
                    {
                        var options = implementationFactory.GetService<IOptions<WorkloadManagerOptions>>();
                        var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                        var logger = implementationFactory.GetService<ILogger<Program>>();
                        return serviceProvider.LocateService<IMessageBrokerPublisherService>(logger, options.Value.Messaging.PublisherServiceAssemblyName);
                    });

                    services.AddSingleton<RabbitMqMessageSubscriberService>();
                    services.AddSingleton<IMessageBrokerSubscriberService>(implementationFactory =>
                    {
                        var options = implementationFactory.GetService<IOptions<WorkloadManagerOptions>>();
                        var serviceProvider = implementationFactory.GetService<IServiceProvider>();
                        var logger = implementationFactory.GetService<ILogger<Program>>();
                        return serviceProvider.LocateService<IMessageBrokerSubscriberService>(logger, options.Value.Messaging.SubscriberServiceAssemblyName);
                    });

                    services.AddSingleton<IEventPayloadRecieverService, EventPayloadRecieverService>();
                    services.AddTransient<IEventPayloadValidator, EventPayloadValidator>();

                    services.AddSingleton<PayloadListenerService>();

                    services.AddHostedService<PayloadListenerService>(p => p.GetService<PayloadListenerService>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
