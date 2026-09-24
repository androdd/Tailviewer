using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
	[TestFixture]
	public sealed class AndFilterTest
	{
		[Test]
		public void TestCtor1()
		{

		}

		[Test]
		public void TestSingleFilter()
		{
			var filter = new AndFilter(new[] {new SubstringFilter("foo", ignoreCase: true)});
			filter.PassesFilter(new LogEntry {RawContent = "foobar"}).Should().BeTrue();
			filter.PassesFilter(new LogEntry {RawContent = "fo bar"}).Should().BeFalse();
		}

		[Test]
		public void TestMultipleFilters()
		{
			var filter = new AndFilter(new[]
			{
				new SubstringFilter("foo", ignoreCase: true),
				new SubstringFilter("bar", ignoreCase: true)
			});
			filter.PassesFilter(new LogEntry {RawContent = "foobar"}).Should().BeTrue();
			filter.PassesFilter(new LogEntry {RawContent = "foo"}).Should().BeFalse();
			filter.PassesFilter(new LogEntry {RawContent = "bar"}).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that each filter is applied to the log entry as a whole: a multi-line entry which matches two filters on different lines passes")]
		public void TestMultiLineEntry()
		{
			var filter = new AndFilter(new[]
			{
				new SubstringFilter("foo", ignoreCase: true),
				new SubstringFilter("bar", ignoreCase: true)
			});

			var logEntry = new List<IReadOnlyLogEntry>
			{
				new LogEntry {RawContent = "foo"},
				new LogEntry {RawContent = "bar"}
			};
			filter.PassesFilter(logEntry).Should().BeTrue("because the entry as a whole contains both filter strings");
		}

		[Test]
		[Description("Verifies that an inverted filter is applied to the log entry as a whole: a multi-line entry which contains the filter string on ANY line must not pass")]
		public void TestMultiLineEntryInvertedFilter()
		{
			var filter = new AndFilter(new ILogEntryFilter[]
			{
				new InvertFilter(new SubstringFilter("Sync", ignoreCase: true)),
				new SubstringFilter("2026", ignoreCase: true)
			});

			var matchingEntry = new List<IReadOnlyLogEntry>
			{
				new LogEntry {RawContent = "2026-09-04 ERROR com.tvm.bl.SyncUserSessionManager - Error mapping local session"},
				new LogEntry {RawContent = "   at java.base/java.lang.reflect.Method.invoke(Method.java:569)"}
			};
			filter.PassesFilter(matchingEntry)
			      .Should().BeFalse("because the entry contains the inverted filter's string on its first line and therefore the entire entry must be excluded");

			var otherEntry = new List<IReadOnlyLogEntry>
			{
				new LogEntry {RawContent = "2026-09-04 ERROR com.tvm.SomethingElse - Some other error"},
				new LogEntry {RawContent = "   at java.base/java.lang.reflect.Method.invoke(Method.java:569)"}
			};
			filter.PassesFilter(otherEntry)
			      .Should().BeTrue("because no line of the entry contains the inverted filter's string");
		}
	}
}
