using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.ReadOnly;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.ReadOnly
{
    public class FieldCanBeMadeReadOnlyRefactoringTests : RefactoringTest
    {
        public FieldCanBeMadeReadOnlyRefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => new FieldCanBeMadeReadOnlyRefactoring(new OptionsService());
        
        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Field is assigned in constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 1;
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in the initializer",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index = 1;
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int index = 1;
    }
}"
                },
                new object[]
                {
                    "Static field is assigned in static constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static int @::@index;

        static TestService()
        {
            index = 1;
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static readonly int index;

        static TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Static field is assigned in the initializer",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static int @::@index = 1;
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static readonly int index = 1;
    }
}"
                },
                new object[]
                {
                    "Cursor placement test (after semicolon)",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int index;@::@

        public TestService()
        {
            index = 1;
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService()
        {
            index = 1;
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
                    "Field is a constant",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private const int @::@index = 1;
    }
}"
                },
                new object[]
                {
                    "Field is alredy readonly",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int @::@index;

        public TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is volatile",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private volatile string @::@value;
    }
}"
                },
                new object[]
                {
                    "Field is never assigned",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;
    }
}"
                },
                new object[]
                {
                    "Static field is never assigned",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static int @::@index;
    }
}"
                },
                new object[]
                {
                    "Field is read in a method",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public int Method()
        {
            return index;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in a method",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public void Method()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in a method and constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in another type (out)",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            var component = new TestComponent();
            component.Method(out index);
        }
    }

    class TestComponent
    {
        public void Method(out int index)
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in another type (ref)",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            var component = new TestComponent();
            component.Method(ref index);
        }
    }

    class TestComponent
    {
        public void Method(ref int index)
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in another type's constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            var component = new TestComponent(this);
        }
    }

    class TestComponent
    {
        public TestComponent(TestService service)
        {
            service.index++;
        }
    }
}"
                },
                new object[]
                {
                    "Field is assigned in constructor and modified in a method",
                    @"
using System;
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            Console.WriteLine(index++);
        }
    }
}"
                },
                new object[]
                {
                    "Static field is assigned in a method",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static int @::@index;

        public void Method()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Static field is assigned in non-static constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private static int @::@index;

        public TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Non-static field is assigned in static constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        static TestService()
        {
            var t = new TestService();
            t.index = 1;
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTest2Data")]
        public Task NoActionTest2(string test, string[] input) =>
            RunNoActionTestAsync(input);

        public static IEnumerable<object[]> NoActionTest2Data =>
            new[]
            {
                new object[]
                {
                    "Field is assigned in another type's constructor (separate documents)",
                    new[] {
                    @"
namespace HelloWorld
{
    class TestService
    {
        public int @::@index;

        public TestService()
        {
            index = 0;
        }

        public void Method()
        {
            var component = new TestComponent(this);
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestComponent
    {
        public TestComponent(TestService service)
        {
            service.index++;
        }
    }
}"
                    }
                }
            };
    }
}
