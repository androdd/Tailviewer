using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using log4net;
using Metrolib;
using Ookii.Dialogs.Wpf;
using Tailviewer.Api;
using Tailviewer.Settings;
using Tailviewer.Ui.Settings.CustomFormats;

namespace Tailviewer.Ui.Settings
{
	public sealed class SettingsFlyoutViewModel
		: IFlyoutViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IReadOnlyList<EncodingViewModel> Encodings;

		static SettingsFlyoutViewModel()
		{
			var encodings = new List<EncodingViewModel>
			{
				// TODO: Move to service
				new EncodingViewModel(null, "Auto detect"),
				new EncodingViewModel(Encoding.Default, $"System default ({Encoding.Default.WebName})"),
				new EncodingViewModel(Encoding.UTF8, "Unicode (UTF-8)"),
				new EncodingViewModel(Encoding.Unicode, "Unicode (UTF-16 little endian)"),
				new EncodingViewModel(Encoding.BigEndianUnicode, "Unicode (UTF-16 big endian)"),
				new EncodingViewModel(Encoding.UTF32, "Unicode (UTF-32)"),
				new EncodingViewModel(Encoding.UTF7, "Unicode (UTF-7)"),
				new EncodingViewModel(Encoding.ASCII, "ASCII (7 bit)")
			};

			// Legacy single-byte code pages which are still in widespread use, most prominently by
			// applications which write text files using the system's default code page of a non-latin locale.
			TryAdd(encodings, codePage: 1251, name: "Cyrillic (windows-1251)");
			TryAdd(encodings, codePage: 20866, name: "Cyrillic (KOI8-R)");
			TryAdd(encodings, codePage: 21866, name: "Cyrillic (KOI8-U)");
			TryAdd(encodings, codePage: 28595, name: "Cyrillic (ISO 8859-5)");
			TryAdd(encodings, codePage: 866, name: "Cyrillic (DOS, cp866)");
			TryAdd(encodings, codePage: 1250, name: "Central European (windows-1250)");
			TryAdd(encodings, codePage: 1252, name: "Western European (windows-1252)");
			TryAdd(encodings, codePage: 1253, name: "Greek (windows-1253)");
			TryAdd(encodings, codePage: 1254, name: "Turkish (windows-1254)");
			TryAdd(encodings, codePage: 28591, name: "Western European (ISO 8859-1)");

			Encodings = encodings;
		}

		private static void TryAdd(ICollection<EncodingViewModel> encodings, int codePage, string name)
		{
			try
			{
				var encoding = Encoding.GetEncoding(codePage);
				// The system default may very well be one of these code pages: We don't want to offer the same
				// encoding twice, so we skip those which are already part of the list.
				if (encodings.Any(x => Equals(x.Encoding, encoding)))
					return;

				encodings.Add(new EncodingViewModel(encoding, name));
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to offer encoding with code page {0} ({1}): {2}", codePage, name, e.Message);
			}
		}

		private readonly IApplicationSettings _settings;
		private readonly LogLevelSettingsViewModel _otherLevel;
		private readonly LogLevelSettingsViewModel _traceLevel;
		private readonly LogLevelSettingsViewModel _debugLevel;
		private readonly LogLevelSettingsViewModel _infoLevel;
		private readonly LogLevelSettingsViewModel _warnLevel;
		private readonly LogLevelSettingsViewModel _errorLevel;
		private readonly LogLevelSettingsViewModel _fatalLevel;
		private string _pluginRepositories;
		private EncodingViewModel _defaultTextFileEncoding;
		private readonly CustomFormatsSettingsViewModel _customFormats;

		public SettingsFlyoutViewModel(IApplicationSettings applicationSettings,
		                                  IServiceContainer serviceContainer)
		{
			_settings = applicationSettings;

			var repos = applicationSettings.AutoUpdate.PluginRepositories;
			_pluginRepositories = repos != null ? string.Join(Environment.NewLine, repos) : string.Empty;

			var defaultEncoding = applicationSettings.LogFile?.DefaultEncoding;
			_defaultTextFileEncoding = Encodings.FirstOrDefault(x => Equals(x.Encoding, defaultEncoding));
			if (_defaultTextFileEncoding == null)
			{
				var @default = TextFileEncodings.FirstOrDefault();
				Log.WarnFormat("Unable to find encoding '{0}', setting default to '{1}'...",  defaultEncoding, @default?.Encoding);

				_defaultTextFileEncoding = @default;
			}

			_otherLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Other);
			_traceLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Trace);
			_debugLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Debug);
			_infoLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Info);
			_warnLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Warning);
			_errorLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Error);
			_fatalLevel = new LogLevelSettingsViewModel(_settings, applicationSettings.LogViewer.Fatal);
			_customFormats = new CustomFormatsSettingsViewModel(_settings, serviceContainer, Encodings);
		}

		public bool CheckForUpdates
		{
			get { return _settings.AutoUpdate.CheckForUpdates; }
			set
			{
				if (value == CheckForUpdates)
					return;

				_settings.AutoUpdate.CheckForUpdates = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public bool AutomaticallyInstallUpdates
		{
			get { return _settings.AutoUpdate.AutomaticallyInstallUpdates; }
			set
			{
				if (value == AutomaticallyInstallUpdates)
					return;

				_settings.AutoUpdate.AutomaticallyInstallUpdates = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyUsername
		{
			get { return _settings.AutoUpdate.ProxyUsername; }
			set
			{
				if (value == ProxyUsername)
					return;

				_settings.AutoUpdate.ProxyUsername = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyPassword
		{
			get { return _settings.AutoUpdate.ProxyPassword; }
			set
			{
				if (value == ProxyPassword)
					return;

				_settings.AutoUpdate.ProxyPassword = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ProxyServer
		{
			get { return _settings.AutoUpdate.ProxyServer; }
			set
			{
				if (value == ProxyServer)
					return;

				_settings.AutoUpdate.ProxyServer = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string ExportFolder
		{
			get { return _settings.Export.ExportFolder; }
			set
			{
				if (value == ExportFolder)
					return;

				_settings.Export.ExportFolder = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public string PluginRepositories
		{
			get { return _pluginRepositories; }
			set
			{
				_pluginRepositories = value;
				_settings.AutoUpdate.PluginRepositories =
					_pluginRepositories?.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public ICommand ChooseExportFolderCommand
		{
			get { return new DelegateCommand2(ChooseExportFolder); }
		}

		public int ScrollSpeed
		{
			get { return _settings.LogViewer.LinesScrolledPerWheelTick; }
			set
			{
				if (value == _settings.LogViewer.LinesScrolledPerWheelTick)
					return;

				_settings.LogViewer.LinesScrolledPerWheelTick = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public int FontSize
		{
			get { return _settings.LogViewer.FontSize; }
			set
			{
				if (value == _settings.LogViewer.FontSize)
					return;

				_settings.LogViewer.FontSize = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}
		
		public int TabWidth
		{
			get { return _settings.LogViewer.TabWidth; }
			set
			{
				if (value == _settings.LogViewer.TabWidth)
					return;

				_settings.LogViewer.TabWidth = value;
				EmitPropertyChanged();

				_settings.SaveAsync();
			}
		}

		public LogLevelSettingsViewModel OtherLevel => _otherLevel;
		public LogLevelSettingsViewModel TraceLevel => _traceLevel;
		public LogLevelSettingsViewModel DebugLevel => _debugLevel;
		public LogLevelSettingsViewModel InfoLevel => _infoLevel;
		public LogLevelSettingsViewModel WarningLevel => _warnLevel;
		public LogLevelSettingsViewModel ErrorLevel => _errorLevel;
		public LogLevelSettingsViewModel FatalLevel => _fatalLevel;

		public CustomFormatsSettingsViewModel CustomFormats => _customFormats;

		public bool AlwaysOnTop
		{
			get { return _settings.MainWindow.AlwaysOnTop; }
			set
			{
				if (value == _settings.MainWindow.AlwaysOnTop)
					return;

				_settings.MainWindow.AlwaysOnTop = value;
				EmitPropertyChanged();

				var app = Application.Current;
				var window = app.MainWindow;
				if (window != null)
				{
					_settings.MainWindow.RestoreTo(window);
				}

				_settings.SaveAsync();
			}
		}

		public bool FolderDataSourceRecursive
		{
			get { return _settings.DataSources.FolderDataSourceRecursive; }
			set
			{
				if (value == _settings.DataSources.FolderDataSourceRecursive)
					return;

				_settings.DataSources.FolderDataSourceRecursive = value;
				_settings.SaveAsync();
			}
		}

		public string FolderDataSourcePatterns
		{
			get { return _settings.DataSources.FolderDataSourcePattern; }
			set
			{
				if (string.Equals(_settings.DataSources.FolderDataSourcePattern, value))
					return;

				_settings.DataSources.FolderDataSourcePattern = value;
				_settings.SaveAsync();
			}
		}

		public IEnumerable<EncodingViewModel> TextFileEncodings => Encodings;

		public EncodingViewModel DefaultTextFileEncoding
		{
			get { return _defaultTextFileEncoding; }
			set
			{
				if (Equals(value, _defaultTextFileEncoding))
					return;

				_defaultTextFileEncoding = value;
				EmitPropertyChanged();

				_settings.LogFile.DefaultEncoding = value?.Encoding;
				_settings.SaveAsync();
			}
		}

		public void Update()
		{}

		public string Name => "Settings";

		private void ChooseExportFolder()
		{
			var dialog = new VistaFolderBrowserDialog
			{
				SelectedPath = ExportFolder,
				Description = "Choose export folder",
				UseDescriptionForTitle = true
			};

			if (dialog.ShowDialog() == true)
			{
				ExportFolder = dialog.SelectedPath;
			}
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}