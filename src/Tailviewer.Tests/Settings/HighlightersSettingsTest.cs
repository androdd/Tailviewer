using System.IO;
using System.Text;
using System.Windows.Media;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class HighlightersSettingsTest
	{
		[Test]
		public void TestConstruction()
		{
			var highlighters = new HighlightersSettings();
			highlighters.Should().BeEmpty();

			var highlighter = new HighlighterSettings();
			highlighter.Id.Should().NotBe(HighlighterId.Empty);
			highlighter.IsActive.Should().BeTrue();
			highlighter.BackgroundColor.Should().NotBe(default(Color));
		}

		[Test]
		public void TestClone()
		{
			var highlighters = new HighlightersSettings
			{
				new HighlighterSettings
				{
					Value = "Sync",
					MatchType = FilterMatchType.WildcardFilter,
					BackgroundColor = Color.FromRgb(1, 2, 3),
					IsActive = false
				}
			};

			var clone = highlighters.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(highlighters);
			clone.Count.Should().Be(1);
			clone[0].Should().NotBeSameAs(highlighters[0]);
			clone[0].Id.Should().Be(highlighters[0].Id);
			clone[0].Value.Should().Be("Sync");
			clone[0].MatchType.Should().Be(FilterMatchType.WildcardFilter);
			clone[0].BackgroundColor.Should().Be(Color.FromRgb(1, 2, 3));
			clone[0].IsActive.Should().BeFalse();
		}

		[Test]
		public void TestStoreRestore()
		{
			using (var stream = new MemoryStream())
			{
				var settings = new HighlightersSettings
				{
					new HighlighterSettings
					{
						Value = "e.*rror",
						MatchType = FilterMatchType.RegexpFilter,
						BackgroundColor = Color.FromRgb(0x90, 0xCA, 0xF9),
						IsActive = false
					},
					new HighlighterSettings
					{
						Value = "Sync"
					}
				};

				using (var writer = XmlWriter.Create(stream, new XmlWriterSettings {Encoding = Encoding.UTF8}))
				{
					writer.WriteStartElement("highlighters");
					settings.Save(writer);
					writer.WriteEndElement();
				}

				stream.Position = 0;
				using (var reader = XmlReader.Create(stream))
				{
					reader.MoveToContent();

					var actualSettings = new HighlightersSettings();
					actualSettings.Restore(reader);

					actualSettings.Count.Should().Be(2);
					actualSettings[0].Id.Should().Be(settings[0].Id);
					actualSettings[0].Value.Should().Be("e.*rror");
					actualSettings[0].MatchType.Should().Be(FilterMatchType.RegexpFilter);
					actualSettings[0].BackgroundColor.Should().Be(Color.FromRgb(0x90, 0xCA, 0xF9));
					actualSettings[0].IsActive.Should().BeFalse();

					actualSettings[1].Id.Should().Be(settings[1].Id);
					actualSettings[1].Value.Should().Be("Sync");
					actualSettings[1].MatchType.Should().Be(FilterMatchType.SubstringFilter);
					actualSettings[1].BackgroundColor.Should().Be(settings[1].BackgroundColor);
					actualSettings[1].IsActive.Should().BeTrue();
				}
			}
		}
	}
}
