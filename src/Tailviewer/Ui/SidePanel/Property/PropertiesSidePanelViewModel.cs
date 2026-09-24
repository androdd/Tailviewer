using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media;
using log4net;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;

namespace Tailviewer.Ui.SidePanel.Property
{
	/// <summary>
	///     Displays the properties of an <see cref="ILogSource" /> and allows the user to edit those which are
	///     not readonly.
	/// </summary>
	internal sealed class PropertiesSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ObservableCollection<IPropertyPresenter> _presenters;
		private readonly Dictionary<IReadOnlyPropertyDescriptor, IPropertyPresenter> _presentersByProperty;
		private readonly PropertiesBufferList _propertyValues;
		private readonly IPropertyPresenterPlugin _registry;
		private ILogSource _logSource;

		public PropertiesSidePanelViewModel(IServiceContainer serviceContainer)
		{
			Tooltip = "Show properties of the current log file";
			_propertyValues = new PropertiesBufferList();
			_presentersByProperty = new Dictionary<IReadOnlyPropertyDescriptor, IPropertyPresenter>();
			_presenters = new ObservableCollection<IPropertyPresenter>();
			_registry = serviceContainer.TryRetrieve<IPropertyPresenterPlugin>();
			if (_registry == null) Log.WarnFormat("Registry is missing: No properties will be displayed...");
		}

		public IReadOnlyList<IPropertyPresenter> Properties
		{
			get { return _presenters; }
		}

		public override Geometry Icon
		{
			get { return Icons.Wrench; }
		}

		public override string Id
		{
			get { return "properties"; }
		}

		public IDataSource CurrentDataSource
		{
			set
			{
				_logSource = value?.UnfilteredLogSource;
				if (_logSource == null)
				{
					_propertyValues.Clear();
				}
			}
		}

		public override void Update()
		{
			_logSource?.GetAllProperties(_propertyValues);
			foreach (var property in _propertyValues.Properties)
			{
				if (!_presentersByProperty.TryGetValue(property, out var presenter))
				{
					presenter = TryCreateViewModel(property);
					_presentersByProperty.Add(property, presenter);

					if (presenter != null)
					{
						// Presenters of writable properties allow the user to change the property's value:
						// Those changes must find their way back into the log source.
						if (property is IPropertyDescriptor writableProperty)
							presenter.OnValueChanged += value => OnPropertyValueChanged(writableProperty, value);

						_presenters.Add(presenter);
					}
				}

				// Some properties just don't have presenters and we want to avoid trying to create one over and over,
				// so we simply remember those that don't have one
				presenter?.Update(_propertyValues.GetValue(property));
			}
		}

		private void OnPropertyValueChanged(IPropertyDescriptor property, object value)
		{
			var logSource = _logSource;
			if (logSource == null)
				return;

			try
			{
				Log.DebugFormat("Setting property {0} to {1}", property, value);
				logSource.SetProperty(property, value);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while setting property {0} to {1}: {2}", property, value, e);
			}
		}

		private IPropertyPresenter TryCreateViewModel(IReadOnlyPropertyDescriptor property)
		{
			try
			{
				return _registry?.TryCreatePresenterFor(property);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return null;
			}
		}
	}
}