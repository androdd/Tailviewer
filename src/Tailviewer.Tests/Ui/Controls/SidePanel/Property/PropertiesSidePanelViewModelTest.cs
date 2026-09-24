using System;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Ui.SidePanel.Property;

namespace Tailviewer.Tests.Ui.Controls.SidePanel.Property
{
	[TestFixture]
	public sealed class PropertiesSidePanelViewModelTest
	{
		private ServiceContainer _services;
		private Mock<IPropertyPresenterPlugin> _registry;
		private Mock<IPropertyPresenter> _encodingPresenter;
		private InMemoryLogSource _logSource;
		private Mock<IDataSource> _dataSource;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();

			_encodingPresenter = new Mock<IPropertyPresenter>();
			_registry = new Mock<IPropertyPresenterPlugin>();
			_registry.Setup(x => x.TryCreatePresenterFor(TextProperties.OverwrittenEncoding))
			         .Returns(_encodingPresenter.Object);
			_services.RegisterInstance<IPropertyPresenterPlugin>(_registry.Object);

			_logSource = new InMemoryLogSource();
			_logSource.SetValue(TextProperties.OverwrittenEncoding, null);

			_dataSource = new Mock<IDataSource>();
			_dataSource.Setup(x => x.UnfilteredLogSource).Returns(_logSource);
		}

		[Test]
		public void TestConstruction()
		{
			var viewModel = new PropertiesSidePanelViewModel(_services);
			viewModel.Properties.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that a presenter is created for each property of the log source and is told about the property's value")]
		public void TestUpdateCreatesPresenters()
		{
			var viewModel = new PropertiesSidePanelViewModel(_services);
			viewModel.CurrentDataSource = _dataSource.Object;
			viewModel.Update();

			viewModel.Properties.Should().Contain(_encodingPresenter.Object);
			_encodingPresenter.Verify(x => x.Update(null), Times.AtLeastOnce);

			var encoding = Encoding.GetEncoding(1251);
			_logSource.SetValue(TextProperties.OverwrittenEncoding, encoding);
			viewModel.Update();
			_encodingPresenter.Verify(x => x.Update(encoding), Times.AtLeastOnce);
		}

		[Test]
		[Description("Verifies that a value chosen by the user via a presenter is written back to the log source")]
		public void TestValueChangedByUserIsAppliedToLogSource()
		{
			var viewModel = new PropertiesSidePanelViewModel(_services);
			viewModel.CurrentDataSource = _dataSource.Object;
			viewModel.Update();

			var encoding = Encoding.GetEncoding(1251);
			_encodingPresenter.Raise(x => x.OnValueChanged += null, (object) encoding);

			_logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().Be(encoding);

			_encodingPresenter.Raise(x => x.OnValueChanged += null, (object) null);
			_logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull();
		}

		[Test]
		[Description("Verifies that a value chosen by the user is applied to the current data source, not the one the presenter was created for")]
		public void TestValueChangedByUserIsAppliedToCurrentLogSource()
		{
			var viewModel = new PropertiesSidePanelViewModel(_services);
			viewModel.CurrentDataSource = _dataSource.Object;
			viewModel.Update();

			var otherLogSource = new InMemoryLogSource();
			otherLogSource.SetValue(TextProperties.OverwrittenEncoding, null);
			var otherDataSource = new Mock<IDataSource>();
			otherDataSource.Setup(x => x.UnfilteredLogSource).Returns(otherLogSource);
			viewModel.CurrentDataSource = otherDataSource.Object;
			viewModel.Update();

			var encoding = Encoding.GetEncoding(1251);
			_encodingPresenter.Raise(x => x.OnValueChanged += null, (object) encoding);

			otherLogSource.GetProperty(TextProperties.OverwrittenEncoding).Should().Be(encoding);
			_logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because the first data source is no longer the current one");
		}

		[Test]
		[Description("Verifies that a value change is ignored when there's no current data source")]
		public void TestValueChangedWithoutDataSource()
		{
			var viewModel = new PropertiesSidePanelViewModel(_services);
			viewModel.CurrentDataSource = _dataSource.Object;
			viewModel.Update();
			viewModel.CurrentDataSource = null;

			new Action(() => _encodingPresenter.Raise(x => x.OnValueChanged += null, (object) Encoding.UTF8))
				.Should().NotThrow();
			_logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull();
		}
	}
}
