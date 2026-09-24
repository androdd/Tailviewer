using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Settings;

namespace Tailviewer.Tests.BusinessLogic.Highlighters
{
	[TestFixture]
	public sealed class HighlighterCollectionTest
	{
		[Test]
		public void TestConstructionFromSettings()
		{
			var settings = new HighlightersSettings
			{
				new HighlighterSettings {Value = "foo"},
				new HighlighterSettings {Value = "bar", IsActive = false}
			};

			var collection = new HighlighterCollection(settings);
			var highlighters = collection.Highlighters.ToList();
			highlighters.Count.Should().Be(2);
			highlighters[0].Value.Should().Be("foo");
			highlighters[0].IsActive.Should().BeTrue();
			highlighters[1].Value.Should().Be("bar");
			highlighters[1].IsActive.Should().BeFalse();
		}

		[Test]
		public void TestAddModifiesSettings()
		{
			var settings = new HighlightersSettings();
			var collection = new HighlighterCollection(settings);

			var changed = 0;
			collection.OnChanged += () => ++changed;

			var highlighter = collection.AddHighlighter();
			settings.Count.Should().Be(1, "because the new highlighter must be backed by the settings so it can be persisted");
			settings[0].Id.Should().Be(highlighter.Id);
			changed.Should().Be(1);

			highlighter.Value = "Sync";
			settings[0].Value.Should().Be("Sync", "because modifications should be reflected in the settings");
		}

		[Test]
		public void TestAddAssignsPaletteColors()
		{
			var collection = new HighlighterCollection(new HighlightersSettings());
			var first = collection.AddHighlighter();
			var second = collection.AddHighlighter();

			first.BackgroundColor.Should().Be(HighlighterCollection.Palette[0]);
			second.BackgroundColor.Should().Be(HighlighterCollection.Palette[1]);
		}

		[Test]
		public void TestRemoveModifiesSettings()
		{
			var settings = new HighlightersSettings();
			var collection = new HighlighterCollection(settings);
			var highlighter = collection.AddHighlighter();

			var changed = 0;
			collection.OnChanged += () => ++changed;

			collection.Remove(highlighter.Id);
			collection.Highlighters.Should().BeEmpty();
			settings.Should().BeEmpty("because the highlighter must have been removed from the settings as well");
			changed.Should().Be(1);
		}
	}
}
