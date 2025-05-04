using System;
using Microsoft.Extensions.Configuration;

namespace Rac.Core.Manager
{
	/// <summary>
	/// Loads and exposes game configuration from config.ini.
	/// </summary>
	public class ConfigManager
	{
		private readonly IConfigurationRoot _configuration;

		public WindowSettings Window { get; }

		public ConfigManager()
		{
			_configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddIniFile("config.ini", optional: true)
				.Build();

			Window = new WindowSettings();
			_configuration.GetSection("window").Bind(Window);
		}
	}

	/// <summary>
	/// Strongly typed settings for the [window] section.
	/// </summary>
	public class WindowSettings
	{
		/// <summary>Window title override.</summary>
		public string? Title { get; set; }

		/// <summary>Override for size, format "width,height".</summary>
		public string? Size { get; set; }

		/// <summary>Override for VSync (true/false).</summary>
		public bool? VSync { get; set; }
	}
}