﻿using Bot.Abstractions;
using Bot.Data;
using Bot.Enums;
using Bot.Models;
using Bot.Services;
using Launch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Runtime.InteropServices;

namespace Launch;

public class Initialize
{
	public static async Task Main()
	{
		try
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("============= Launching =============");

			var builder = WebApplication.CreateBuilder();

			var cachedServices = new CachedServices();

			builder.Services.AddSingleton(cachedServices);

			var isEdit = ShouldEdit(builder.Environment);

			ConsoleCreator.AddHeading("Getting Client Id");
			var clientId = GetClientId(isEdit);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Getting Database Info");
			var database = GetDatabaseOptions(isEdit);
			Console.ResetColor();

			AppSettings appSettings;

			try
			{
				ConsoleCreator.AddHeading("Getting App Settings");
				appSettings = await GetAppSettings(isEdit, clientId, database);
				Console.ResetColor();

				builder.Services.AddSingleton(appSettings);
			}
			catch (MySqlException e)
			{
				Console.WriteLine();

				ConsoleCreator.AddSubHeading("Failed to get app settings",
					$"{e.Message} (MySqlException)");

				ConsoleCreator.AddHeading("Trying to add migrations, in the case of an error!");

				await TryAddMigrations(cachedServices, builder, database);

				Console.ResetColor();
				return;
			}

			ConsoleCreator.AddHeading("Get Modules");
			var modules = GetModules();
			Console.ResetColor();

			ConsoleCreator.AddHeading("Importing Modules");
			InitializeModules(modules, database, cachedServices, appSettings, builder);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Get Authorization Policies");
			var authorizationPolicies = GetAuthorizationPolicies(modules);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Initializing Web Modules");
			InitializeWeb(cachedServices, builder, authorizationPolicies);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Building Application");
			var app = builder.Build();
			Console.ResetColor();

			ConsoleCreator.AddHeading("Adding Migrations");
			await AddMigrations(cachedServices, app);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Configuring App");
			ConfigureApp(modules, cachedServices, appSettings, authorizationPolicies, app);
			Console.ResetColor();

			ConsoleCreator.AddHeading("Running App");

			await app.RunAsync();
		}
		catch (Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(e.ToString());
		}

		Console.ResetColor();
	}

	private static bool ShouldEdit(IWebHostEnvironment env)
	{
		try
		{
			var _ = Console.KeyAvailable;
		}
		catch (InvalidOperationException)
		{
			return false;
		}

		if (!env.IsDevelopment())
			return ConsoleCreator.WaitForUser($"edit settings", 10);
		else
			return false;
	}

	private static ulong GetClientId(bool isEdit)
	{
		while (true)
		{
			var client =
				ConsoleCreator.AskAndSet("Discord OAuth Client ID", "DISCORD_OAUTH_CLIENT_ID", !isEdit);
			if (ulong.TryParse(client.Key, out var clientId))
			{
				ConsoleCreator.AddSubHeading("Found Client ID", clientId.ToString());
				return clientId;
			}
		}
	}

	private static async Task TryAddMigrations(CachedServices cachedServices,
		WebApplicationBuilder builder, Action<DbContextOptionsBuilder> database)
	{
		foreach (var type in cachedServices.GetClassTypes<DataContextCreate>())
			type.GetMethod("AddContextToServiceProvider")?.Invoke(null, new object[] { database, builder.Services });

		var app = builder.Build();

		Console.WriteLine();

		await AddMigrations(cachedServices, app);

		ConsoleCreator.AddHeading("Exiting Application");
	}

	private static Action<DbContextOptionsBuilder> GetDatabaseOptions(bool isEdit)
	{
		var (databaseSettings, hasUpdatedDbSettings) = ConsoleCreator.CreateDatabaseSettings(isEdit);

		if (hasUpdatedDbSettings)
			ConsoleCreator.AddSubHeading("You are finished creating the database settings for", databaseSettings.User);
		else
			ConsoleCreator.AddSubHeading("Found database settings for", $"{databaseSettings.User} // {databaseSettings.Database}");

		ConsoleCreator.AddSubHeading("Successfully created", "MySQL database provider");

		var connectionString = $"Server={databaseSettings.Host};Port={databaseSettings.Port};" +
			$"Database={databaseSettings.Database};Uid={databaseSettings.User};Pwd={databaseSettings.Pass};";

		var dbBuilder = new DbContextOptionsBuilder<BotDatabase>();

		return x => x.UseMySql(
			connectionString,
			ServerVersion.AutoDetect(connectionString),
			o =>
			{
				o.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}");
				o.EnableRetryOnFailure();
			}
		);
	}

	private static async Task<AppSettings> GetAppSettings(bool isEdit, ulong clientId, Action<DbContextOptionsBuilder> dbOptions)
	{
		AppSettings settings;

		ConsoleCreator.AddSubHeading("Querying database for", nameof(AppSettings));

		var dbBuilder = new DbContextOptionsBuilder<BotDatabase>();

		dbOptions.Invoke(dbBuilder);

		await using (var dataContext = new BotDatabase(dbBuilder.Options))
		{
			await dataContext.Database.MigrateAsync();

			var appSettingRepo = new SettingsRepository(dataContext, new AppSettings() { ClientId = clientId }, null);

			settings = await appSettingRepo.GetAppSettings();

			if (settings is null)
			{
				ConsoleCreator.AddHeading("Running First Time Setup");

				ConsoleCreator.AddSubHeading("Welcome to", "Dexter!");
				ConsoleCreator.AddSubHeading("Support Discord", "https://discord.gg/DBS664yjWN");

				settings = ConsoleCreator.CreateAppSettings(clientId, true);

				await appSettingRepo.AddAppSetting(settings);

				ConsoleCreator.AddSubHeading("You are finished creating the app settings for client", clientId.ToString());

				ConsoleCreator.AddSubHeading("You can now access the panel at", settings.GetServiceUrl());

				ConsoleCreator.AddSubHeading("You can always change these settings", "by pressing any key on next reboot");
			}
			else
			{
				ConsoleCreator.AddSubHeading("Found app settings for client", clientId.ToString());

				settings = ConsoleCreator.CreateAppSettings(clientId, isEdit);

				if (isEdit)
					await appSettingRepo.UpdateAppSetting(settings);

				Console.WriteLine();
			}
		}

		return settings;
	}

	private static List<Module> GetModules()
	{
		var modules = ImportModules.GetModules();

		foreach (var module in modules)
		{
			ConsoleCreator.AddSubHeading("Imported", $"{module.GetType().Namespace}{(module is WebModule ? " (WEB)" : "")}");

			if (module.Contributors.Length > 0)
			{
				Console.Write("   Contributed By:      ");

				Console.ForegroundColor = ConsoleColor.DarkMagenta;
				Console.WriteLine(string.Join(", ", module.Contributors));
			}

			Console.ResetColor();
		}

		return modules;
	}

	private static List<string> GetAuthorizationPolicies(List<Module> modules)
	{
		var authorizationPolicies = new List<string>();

		foreach (var startup in modules)
			if (startup is WebModule module)
			{
				var authorizationPolicy = module.AddAuthorizationPolicy();

				if (authorizationPolicy.Length > 0)
					authorizationPolicies = authorizationPolicies.Union(authorizationPolicy).ToList();
			}

		ConsoleCreator.AddSubHeading("Successfully added", "authentication policies");

		return authorizationPolicies;
	}

	private static void InitializeModules(List<Module> modules, Action<DbContextOptionsBuilder> dbOptions,
		CachedServices cachedServices, AppSettings appSettings, WebApplicationBuilder builder)
	{
		try
		{
			foreach (var startup in modules)
				startup.AddLogging(builder.Logging);

			ConsoleCreator.AddSubHeading("Successfully initialized", "logging");

			foreach (var startup in modules)
				startup.AddPreServices(builder.Services, cachedServices, dbOptions);

			ConsoleCreator.AddSubHeading("Successfully initialized", "pre-services");

			foreach (var startup in modules)
				startup.AddServices(builder.Services, cachedServices, appSettings);

			ConsoleCreator.AddSubHeading("Successfully initialized", "services");

			foreach (var startup in modules)
				startup.ConfigureServices(builder.Configuration, builder.Services);

			ConsoleCreator.AddSubHeading("Successfully configured", "services");
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.ToString());
			return;
		}
	}

	private static void InitializeWeb(CachedServices cachedServices, WebApplicationBuilder builder,
		List<string> authorizationPolicies)
	{
		builder.WebHost.CaptureStartupErrors(true);

		builder.WebHost.UseUrls("http://0.0.0.0:80/");

		builder.Services.AddMemoryCache();

		var controller = builder.Services.AddControllers()
			.AddNewtonsoftJson(x =>
			{
				x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				x.SerializerSettings.Converters.Add(new UlongConverter());
			});

		foreach (var assembly in cachedServices.Dependents)
			controller.AddApplicationPart(assembly);

		builder.Services.AddAuthorization(options =>
		{
			options.DefaultPolicy = new AuthorizationPolicyBuilder(authorizationPolicies.ToArray())
				.RequireAuthenticatedUser().Build();
		});

		ConsoleCreator.AddSubHeading("Started authorization policies for", string.Join(',', authorizationPolicies));
	}

	private static async Task AddMigrations(CachedServices cachedServices, WebApplication app)
	{
		ConsoleCreator.AddSubHeading("Heads up!", "This might take a while on a first install...\n");

		using (var scope = app.Services.CreateScope())
		{
			foreach (var dataContext in cachedServices.GetInitializedClasses<DbContext>(scope.ServiceProvider))
			{
				ConsoleCreator.AddSubHeading("Adding migrations for", dataContext.GetType().Name);

				await dataContext.Database.MigrateAsync();
			}
		}

		Console.WriteLine();

		ConsoleCreator.AddSubHeading("Successfully added", "migrations to databases");
	}

	private static void ConfigureApp(List<Module> modules, CachedServices cachedServices, AppSettings appSettings,
		List<string> authorizationPolicies, WebApplication app)
	{
		app.UseAuthentication();

		foreach (var startup in modules)
		{
			startup.PostBuild(app.Services, cachedServices);

			if (startup is WebModule module)
				module.PostWebBuild(app, appSettings);
		}

		ConsoleCreator.AddSubHeading("Successfully post built", "application");

		if (appSettings.EncryptionType == EncryptionType.HTTPS)
			app.UseHttpsRedirection();

		app.UseRouting();

		if (authorizationPolicies.Any())
			app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}