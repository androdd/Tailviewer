using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core;

namespace Tailviewer.Ui.SidePanel.Highlighters
{
	public sealed class HighlighterViewModel
		: INotifyPropertyChanged
	{
		private readonly Highlighter _highlighter;
		private readonly ICommand _removeCommand;
		private bool _isValid;
		private bool _isEditing;

		public HighlighterViewModel(Highlighter highlighter, Action<HighlighterViewModel> onRemove)
		{
			_highlighter = highlighter;
			_removeCommand = new DelegateCommand2(() => onRemove(this));

			UpdateValidity();
		}

		public Highlighter Highlighter => _highlighter;

		public bool IsActive
		{
			get { return _highlighter.IsActive; }
			set
			{
				if (value == _highlighter.IsActive)
					return;

				_highlighter.IsActive = value;
				EmitPropertyChanged();
			}
		}

		public bool IsValid
		{
			get { return _isValid; }
			private set
			{
				if (value == _isValid)
					return;

				_isValid = value;
				EmitPropertyChanged();
			}
		}

		public string Value
		{
			get { return _highlighter.Value; }
			set
			{
				if (value == _highlighter.Value)
					return;

				_highlighter.Value = value;
				UpdateValidity();
				EmitPropertyChanged();
			}
		}

		public bool IsEditing
		{
			get { return _isEditing; }
			set
			{
				if (value == _isEditing)
					return;

				_isEditing = value;
				EmitPropertyChanged();
			}
		}

		public FilterMatchType MatchType
		{
			get { return _highlighter.MatchType; }
			set
			{
				if (value == _highlighter.MatchType)
					return;

				_highlighter.MatchType = value;
				UpdateValidity();
				EmitPropertyChanged();
			}
		}

		public Color Color
		{
			get { return _highlighter.BackgroundColor; }
			set
			{
				if (value == _highlighter.BackgroundColor)
					return;

				_highlighter.BackgroundColor = value;
				EmitPropertyChanged();
			}
		}

		public IReadOnlyList<Color> AvailableColors => HighlighterCollection.Palette;

		public ICommand RemoveCommand => _removeCommand;

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateValidity()
		{
			try
			{
				_highlighter.CreateFilter();
				IsValid = true;
			}
			catch (ArgumentException)
			{
				IsValid = false;
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
