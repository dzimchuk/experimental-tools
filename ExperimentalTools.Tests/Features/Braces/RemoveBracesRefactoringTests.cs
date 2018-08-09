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

        [Theory, MemberData(nameof(HasActionTestData))]
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
            {@::@
                i = i + 1;
            }
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
                i = i + 1;
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
            {@::@
                i = i + 1;
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
        private int i;

        public void TestMethod()
        {
            lock(this)
                i = i + 1;
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
            {@::@
                fs.ReadByte();
            }
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
                fs.ReadByte();
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
            {@::@
                Console.WriteLine(*p);
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
        public unsafe void TestMethod()
        {
            string str = ""Hello World"";

            fixed (char* p = str)
                Console.WriteLine(*p);
        }
    }
}"
                },
                new object[]
                {
                    "No else clause escape case",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public string TestMethod()
        {
            if (true)@::@
            {
                if (false)
                    return ""A"";
                else
                    return ""D"";
            }
            else
                return ""B"";

            return ""C"";
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public string TestMethod()
        {
            if (true)
                if (false)
                    return ""A"";
                else
                    return ""D"";
            else
                return ""B"";

            return ""C"";
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
                new object[]
                {
                    "Else clause escape case (parent statement)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public string TestMethod()
        {
            if (true)@::@
            {
                if (false)
                    return ""A"";
            }
            else
                return ""B"";

            return ""C"";
        }
    }
}"
                },
                new object[]
                {
                    "Else clause escape case (inner statement)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public string TestMethod()
        {
            if (true)
            {
                @::@if (false)
                    return ""A"";
            }
            else
                return ""B"";

            return ""C"";
        }
    }
}"
                },
                new object[]
                {
                    "Else clause escape case (block)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public string TestMethod()
        {
            if (true)
            {@::@
                if (false)
                    return ""A"";
            }
            else
                return ""B"";

            return ""C"";
        }
    }
}"
                }
            };
    }
}
