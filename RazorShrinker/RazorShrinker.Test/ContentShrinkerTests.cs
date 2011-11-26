namespace RazorShrinker.Test {
	using NUnit.Framework;
	using RazorShrinker.NAntTask;

	[TestFixture]
	public class ContentShrinkerTests {

		[TestCase( "", (string)null )]
		[TestCase( (string)null, null )]
		[TestCase( "   ", (string)null )]
		[TestCase( "\t\t", (string)null )]
		[TestCase( "\ta\t", "a" )]
		[TestCase( "\n\n", (string)null )]
		[TestCase( "\na\n", "a" )]
		[TestCase( "a\n\n\na", "a a" )]
		[TestCase( "\r\na\r\n", "a" )]
		[TestCase( "\r\r\ra\n\n\n", "a" )]
		[TestCase( "<something>\n<something>", "<something><something>" )]
		[TestCase( "<something>\nsomething", "<something> something" )]
		[TestCase( "something\n<something>", "something <something>" )]
		[TestCase( "abc@*123*@def", "abcdef" )]
		[TestCase( @"5678@*
			def
			ghi
			jkl
			*@mno
			pqr@*abc*@
			stu", @"5678mno pqr stu" )]
		[TestCase( "abc<script type=\"text/javascript\">123</script>def", "abc<script type=\"text/javascript\">123</script>def" )]
		[TestCase( "abc<script type=\"text/javascript\"></script>def", "abc<script type=\"text/javascript\"></script>def" )]
		[TestCase( "abc<script>123/*PQR*/456</script>def", "abc<script>123456</script>def" )]
		[TestCase( "abc<script>123</script>def", "abc<script>123</script>def" )]
		[TestCase( "abc<script type=\"text/javascript\">123/*abc*/456</script>def", "abc<script type=\"text/javascript\">123456</script>def" )]
		[TestCase( "abc<script type=\"text/javascript\">123/*abc*/456/*def*/789</script>def", "abc<script type=\"text/javascript\">123456789</script>def" )]
		[TestCase( @"abc<script type=""text/javascript"">123
			/*abc*/
			456
			</script>def", "abc<script type=\"text/javascript\">123 456 </script>def" )]
		[TestCase( @"abc<script type=""text/javascript"">123
			/*
			abc
			*/
			456
			</script>def", "abc<script type=\"text/javascript\">123 456 </script>def" )]
		[TestCase( @"abc<script type=""text/javascript"">
			123/*
			abc
			*/456
			</script>def", "abc<script type=\"text/javascript\"> 123456 </script>def" )]
		[TestCase( @"abc<script>
			123/*
			abc
			*/456
			</script>def", "abc<script> 123456 </script>def" )]
		[TestCase( @"5678/*
			def
			ghi
			jkl
			*/mno
			pqr/*abc*/
			stu", @"5678/* def ghi jkl */mno pqr/*abc*/ stu" )]
		[TestCase( @"56<script>78/*
			def
			ghi
			jkl
			*/mno
			pqr/*abc*/
			stu
			</script>", @"56<script>78mno pqr stu </script>" )]
		[TestCase( @"56<style>78/*
			def
			ghi
			jkl
			*/mno
			pqr/*abc*/
			stu
			</style>", @"56<style>78mno pqr stu </style>" )]
		[TestCase( @"56<script>78
			//def
			// ghi
			90//jkl
			123
			</script>", @"56<script>78 90 123 </script>" )]
		[TestCase( @"<script>
			//def
			</script>", @"<script> </script>" )]
		[TestCase( @"<script></script>
			//def
			<script></script>", @"<script></script> //def <script></script>" )]
		[TestCase( "ab<script>cde</script>fgh/*ijk*/lmn<script>opq</script>rst", "ab<script>cde</script>fgh/*ijk*/lmn<script>opq</script>rst" )]
		[TestCase( "ab<style>cde</style>fgh/*ijk*/lmn<style>opq</style>rst", "ab<style>cde</style>fgh/*ijk*/lmn<style>opq</style>rst" )]
		public void ShrinkerTests( string Source, string Expected ) {
			ContentShrinker cs = new ContentShrinker();
			string results = cs.ShrinkContent( Source );
			Assert.That( results, Is.EqualTo( Expected ) );
		}

	}
}