using ExperimentalTools.Tests.Infrastructure.Refactoring;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Roslyn.Features.Braces;
using ExperimentalTools.Options;
using Xunit;

namespace ExperimentalTools.Tests.Features.Braces
{
    public class RemoveBracesRefactoringTests : RefactoringTest
    {
        public RemoveBracesRefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => 
            new RemoveBracesRefactoring(new OptionsService());

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inner statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
            {
                throw new A@::@rgumentNullException(nameof(arg));
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));
        }
    }
}"
                },
                new object[]
                {
                    "Block",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
            {@::@
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));
        }
    }
}"
                },
                new object[]
                {
                    "Parent statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            @::@if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));
        }
    }
}"
                },
                new object[]
                {
                    "While statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int i)
        {
            while (true)
            {@::@
                i++;
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int i)
        {
            while (true)
                i++;
        }
    }
}"
                },
                new object[]
                {
                    "For statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int i)
        {
            for (int n = 0; n < 10; n++)
            {@::@
                i++;
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int i)
        {
            for (int n = 0; n < 10; n++)
                i++;
        }
    }
}"
                },
                new object[]
                {
                    "Foreach statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int[] arr)
        {
            foreach(var n in arr)
            {@::@
                Console.WriteLine(n);
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int[] arr)
        {
            foreach(var n in arr)
                Console.WriteLine(n);
        }
    }
}"
                },
                new object[]
                {
                    "Else if statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else if (arg == ""1"")
            {@::@
                arg = ""2"";
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else if (arg == ""1"")
                arg = ""2"";
        }
    }
}"
                },
                new object[]
                {
                    "Else clause (inside inner statement)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
            {
                @::@arg = ""2"";
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
                arg = ""2"";
        }
    }
}"
                },
                new object[]
                {
                    "Else clause (inside block)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
            {@::@
                arg = ""2"";
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
                arg = ""2"";
        }
    }
}"
                },
                new object[]
                {
                    "Else clause",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            el@::@se
            {
                arg = ""2"";
            }
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
                arg = ""2"";
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string input) =>
            RunNoActionTestAsync(input);

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside a statement which is not in a block",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg != null)
                Console.Write@::@Line(arg);
        }
    }
}"
                },
                new object[]
                {
                    "Inside one of the statements of a multistatement block",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg != null)
            {
                arg = arg + ""1"";
                Console.Wri@::@teLine(arg);
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside a block with multiple statements",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg != null)
            {@::@
                arg = arg + ""1"";
                Console.WriteLine(arg);
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside a parent statement with multiple statements",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            @::@if (arg != null)
            {
                arg = arg + ""1"";
                Console.WriteLine(arg);
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside an empty parent statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            @::@if (arg != null)
            {
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside an empty block",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg != null)
            {@::@
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside one of the statements of a multistatement else clause",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
            {
                @::@arg = ""2"";
                Console.WriteLine(arg);
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside a block of a multistatement else clause",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            else
            {@::@
                arg = ""2"";
                Console.WriteLine(arg);
            }
        }
    }
}"
                },
                new object[]
                {
                    "Multistatement else clause",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                arg = """";
            el@::@se
            {
                arg = ""2"";
                Console.WriteLine(arg);
            }
        }
    }
}"
                },
            };
    }
}
