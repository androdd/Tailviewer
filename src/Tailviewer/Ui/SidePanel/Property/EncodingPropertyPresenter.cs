using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using log4net;
using Tailviewer.Api;
using Tailviewer.Ui.Settings;

namespace Tailviewer.Ui.SidePanel.Property
{
	public sealed class EncodingPropertyPresenter
		: IPropertyPresenter
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _displayName;
		private readonly ComboBox _comboBox;
		private readonly List<EncodingViewModel> _encodings;
		private bool _isUpdating;
		private object _lastUnknownValue;

		public EncodingPropertyPresenter(string displayName)
		{
			_displayName = displayName;
			_encodings = new List<EncodingViewModel>(SettingsFlyoutViewModel.Encodings);

			_comboBox = new ComboBox
			{
				ItemsSource = _encodings
			};
			_comboBox.SelectionChanged += ComboBoxOnSelectionChanged;
		}

		private void ComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// We only want to forward changes made by the user, not the ones we make ourselves
			// when we're being told the current value of the property.
			if (_isUpdating)
				return;

			var viewModel = _comboBox.SelectedItem as EncodingViewModel;
			EmitOnValueChanged(viewModel?.Encoding);
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Implementation of IPropertyPresenter

		public string DisplayName => _displayName;

		public object Value
		{
			get
			{
				return _comboBox;
			}
		}

		public void Update(object newValue)
		{
			// A null encoding means that the user hasn't overwritten the encoding, which is represented
			// by the "Auto detect" entry (whose Encoding is null as well).
			var viewModel = _encodings.FirstOrDefault(x => Equals(x.Encoding, newValue));
			if (viewModel == null)
			{
				// We're being updated periodically, so we only want to complain once per unknown value
				if (!Equals(newValue, _lastUnknownValue))
				{
					Log.WarnFormat("No model found for encoding: {0}", newValue);
					_lastUnknownValue = newValue;
				}
				return;
			}

			if (Equals(_comboBox.SelectedItem, viewModel))
				return;

			try
			{
				_isUpdating = true;
				_comboBox.SelectedItem = viewModel;
			}
			finally
			{
				_isUpdating = false;
			}
		}

		public event Action<object> OnValueChanged;

		#endregion

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void EmitOnValueChanged(object obj)
		{
			OnValueChanged?.Invoke(obj);
		}
	}
}