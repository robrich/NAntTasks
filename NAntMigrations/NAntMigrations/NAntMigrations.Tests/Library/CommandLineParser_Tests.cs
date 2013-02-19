namespace NAnt.DbMigrations.Tasks.Tests.Library {
	using System;
	using System.Collections.Generic;
	using NAnt.DbMigrations.Tasks.Library;
	using NUnit.Framework;

	[TestFixture]
	public class CommandLineParser_Tests : TestBase {

		[Test]
		public void MatchesParameterWithValue() {

			// Arrange
			string key = "thekey";
			string expectedValue = "thevalue";
			string[] args = new string[] { "--" + key + "=" + expectedValue };
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void MatchesParameterWithOutValue() {

			// Arrange
			string key = "thekey";
			string expectedValue = "";
			string[] args = new string[] { "--" + key }; // No =something
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void MatchesDuplicateParameters() {

			// Arrange
			string key = "thekey";
			string expectedValue = "thevalue";
			List<string> expectedValues = new List<string> {expectedValue, expectedValue};
			string[] args = new string[] { "--" + key + "=" + expectedValue, "--" + key + "=" + expectedValue };
			int expectedMatches = 2;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValues, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void MatchesParameterAbbreviation() {

			// Arrange
			string shortkey = "thekey";
			string expectedValue = "thevalue";
			string[] args = new string[] { "-" + shortkey + "=" + expectedValue };
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( "notthekey", expectedValue, () => called = true, shortkey );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void MatchesParameterValueHasSpaces() {

			// Arrange
			string key = "thekey";
			string expectedValue = @"c:\the value has\spaces in the value\that would trip\other systems\with long values";
			string[] args = new string[] { "--" + key + "=" + expectedValue };
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void MatchesParameterSlashLeadCharacters() {

			// Arrange
			string key = "thekey";
			string expectedValue = "thevalue";
			string[] args = new string[] { "/" + key + "=" + expectedValue };
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void IgnoresIrrelevantParameters() {

			// Arrange
			string key = "thekey";
			string expectedValue = "thevalue";
			string[] args = new string[] { "--" + key + "=" + expectedValue, "--irrelevant=parameter" };
			int expectedMatches = 1;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( true ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Test]
		public void IgnoresAllParametersWhenIrrelevant() {

			// Arrange
			string key = "thekey";
			string expectedValue = "thevalue";
			string[] args = new string[] { "--irrelevant=parameter" };
			int expectedMatches = 0;
			bool called = false;
			MockArgParameter parm = new MockArgParameter( key, expectedValue, () => called = true );

			// Act
			int actualMatches = CommandLineParser.ParseCommandLineArguments( args, parm );

			// Assert
			Assert.That( called, Is.EqualTo( false ) );
			Assert.That( actualMatches, Is.EqualTo( expectedMatches ) );
		}

		[Ignore]
		public class MockArgParameter : ArgParameter {

			public MockArgParameter( string Name, List<string> ExpectedValues, Action Callback, string Abbreviation = null )
				: base( Name, Abbreviation, v => {
					Callback();
					Assert.That( v, Is.Not.Null );
					Assert.That( v.Count, Is.EqualTo( ExpectedValues.Count ) );
					Assert.That( v, Is.EquivalentTo( ExpectedValues ) );
				} ) {
			}

			public MockArgParameter( string Name, string ExpectedValue, Action Callback, string Abbreviation = null )
				: this( Name, new List<string> { ExpectedValue }, Callback, Abbreviation ) {
			}

		}

	}
}
