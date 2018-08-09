using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.Braces;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.Braces
{
    public class AddBracesRefactoringTests : RefactoringTest
    {
        public AddBracesRefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => new AddBracesRefactoring(new OptionsService());

        [Theory, MemberData(nameof(HasActionTestData))]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Before inner statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
            @::@    throw new ArgumentNullException(nameof(arg));
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
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}"
                },
                new object[]
                {
                    "After inner statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));@::@
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
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside inner statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (arg == null)
                throw new Argument@::@NullException(nameof(arg));
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
            {
                throw new ArgumentNullException(nameof(arg));
            }
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
            @::@    i = i + 1;
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
            {
                i = i + 1;
            }
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
            @::@    arg = ""2"";
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
            {
                arg = ""2"";
            }
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
            @::@    arg = ""2"";
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
            {
                arg = ""2"";
            }
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
            else@::@
                arg = ""2"";
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
            {
                arg = ""2"";
            }
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
            @::@    i = i + 1;
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
            {
                i = i + 1;
            }
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
            foreach (int i in arr)
            @::@    i = i + 1;
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
            foreach (int i in arr)
            {
                i = i + 1;
            }
        }
    }
}"
                },
                new object[]
                {
                    "Do statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(int i)
        {
            do
            @::@    i = i + 1;
            while (true);
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
            do
            {
                i = i + 1;
            }
            while (true);
        }
    }
}"
                },
                new object[]
                {
                    "Lock statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int i;

        public void TestMethod()
        {
            lock(this)
            @::@    i = i + 1;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int i;

        public void TestMethod()
        {
            lock(this)
            {
                i = i + 1;
            }
        }
    }
}"
                },
                new object[]
                {
                    "Using statement",
                    @"
using System;
using System.IO;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod()
        {
            using (var fs = new FileStream("""", FileMode.Open))
            @::@    fs.ReadByte();
        }
    }
}",
                    @"
using System;
using System.IO;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod()
        {
            using (var fs = new FileStream("""", FileMode.Open))
            {
                fs.ReadByte();
            }
        }
    }
}"
                },
                new object[]
                {
                    "Fixed statement",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public unsafe void TestMethod()
        {
            string str = ""Hello World"";

            fixed (char* p = str)
            @::@    Console.WriteLine(*p);
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public unsafe void TestMethod()
        {
            string str = ""Hello World"";

            fixed (char* p = str)
            {
                Console.WriteLine(*p);
            }
        }
    }
}"
                }
            };

        [Theory, MemberData(nameof(NoActionTestData))]
        public Task NoActionTest(string test, string input) =>
            RunNoActionTestAsync(input);

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside a block",
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
}"
                },
                new object[]
                {
                    "Inside a statement nested inside a block",
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
                throw@::@ new ArgumentNullException(nameof(arg));
            }
        }
    }
}"
                },
                new object[]
                {
                    "Inside a parent statement with a block",
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
}"
                },
                new object[]
                {
                    "Inside else clause with a block",
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
            els@::@e
            {
                arg = ""2"";
            }
        }
    }
}"
                },
                new object[]
                {
                    "Before parent statement (IDE0011)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            @::@if (arg == null)
                throw new ArgumentNullException(nameof(arg));
        }
    }
}"
                },
                new object[]
                {
                    "Inside parent statement (IDE0011)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void TestMethod(string arg)
        {
            if (ar@::@g == null)
                throw new ArgumentNullException(nameof(arg));
        }
    }
}"
                }
            };
    }
}
