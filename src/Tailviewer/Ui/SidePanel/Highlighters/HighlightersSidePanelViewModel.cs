using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Settings;

namespace Tailviewer.Ui.SidePanel.Highlighters
{
	/// <summary>
	///     Responsible for presenting the list of highlighters to the user who is able
	///     to add new, change existing ones and delete them.
	/// </summary>
	public sealed class HighlightersSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private readonly IApplicationSettings _applicationSettings;
		private readonly DelegateCommand2 _addCommand;
		private readonly IHighlighters _highlighters;
		private readonly ObservableCollection<HighlighterViewModel> _highlighterViewModels;

		public HighlightersSidePanelViewModel(IApplicationSettings applicationSettings, IHighlighters highlighters)
		{
			_applicationSettings = applicationSettings;
			_highlighters = highlighters;
			_highlighterViewModels = new ObservableCollection<HighlighterViewModel>();
			foreach (var highlighter in _highlighters.Highlighters)
			{
				AddHighlighterViewModel(highlighter);
			}

			_addCommand = new DelegateCommand2(Add);

			UpdateQuickInfo();
		}

		public IEnumerable<HighlighterViewModel> Highlighters
		{
			get { return _highlighterViewModels; }
		}

		public ICommand AddCommand
		{
			get { return _addCommand; }
		}

		private void Add()
		{
			var highlighter = _highlighters.AddHighlighter();
			var viewModel = AddHighlighterViewModel(highlighter);
			viewModel.IsEditing = true;
			UpdateQuickInfo();
			_applicationSettings.SaveAsync();
		}

		private HighlighterViewModel AddHighlighterViewModel(Highlighter highlighter)
		{
			var viewModel = new HighlighterViewModel(highlighter, OnRemove);
			viewModel.PropertyChanged += HighlighterViewModelOnPropertyChanged;
			_highlighterViewModels.Add(viewModel);
			return viewModel;
		}

		private void HighlighterViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(HighlighterViewModel.IsEditing):
				case nameof(HighlighterViewModel.IsValid):
					// Pure UI state which doesn't influence what is being highlighted...
					break;

				default:
					var viewModel = (HighlighterViewModel) sender;
					_highlighters.Update(viewModel.Highlighter);
					_applicationSettings.SaveAsync();
					break;
			}
		}

		private void OnRemove(HighlighterViewModel viewModel)
		{
			viewModel.PropertyChanged -= HighlighterViewModelOnPropertyChanged;
			_highlighterViewModels.Remove(viewModel);
			_highlighters.Remove(viewModel.Highlighter.Id);
			UpdateQuickInfo();
			_applicationSettings.SaveAsync();
		}

		private void UpdateQuickInfo()
		{
			var count = _highlighterViewModels.Count;
			QuickInfo = count > 0
				? string.Format("{0} highlighter{1}", count, count != 1 ? "s" : "")
				: null;
		}

		#region Overrides of AbstractSidePanelViewModel

		public override Geometry Icon
		{
			get { return Icons.Marker; }
		}

		public override string Id
		{
			get { return "highlighters"; }
		}

		public override void Update()
		{
		}

		#endregion
	}
}
