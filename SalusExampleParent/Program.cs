﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Salus;
using SalusExampleParent;

using IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSalus<ExampleDbContext>(options =>
        {
            options.SetMessageSender(message => Console.WriteLine("Sending: " + JsonConvert.SerializeObject(message)));
        });
        services.AddScoped<ExampleParent>();
        services.AddDbContext<ExampleDbContext>(options =>
        {
            options.UseSqlite("Data Source=Application.db");
        });
    })
    .Build();

using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
context.Database.OpenConnection();
context.Database.Migrate();

await host.StartAsync();

var app = scope.ServiceProvider.GetRequiredService<ExampleParent>();
await app.Run();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.StopApplication();
await host.WaitForShutdownAsync();