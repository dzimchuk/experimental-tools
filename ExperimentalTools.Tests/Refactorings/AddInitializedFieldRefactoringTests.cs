using ExperimentalTools.Components;
using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class AddInitializedFieldRefactoringTests
    {
        [Theory, MemberData("HasActionTestData")]
        public async Task HasActionTest(string test, string input, string expectedOutput)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddInitializedFieldRefactoring(new SimpleNameGenerator());
            await provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);
            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside parameter name (1)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(int @::@index)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Inside parameter name (2)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(string name, int index@::@)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(string name, int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Inside parameter name (3)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(string name@::@, int index)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly string name;

        public TestService(string name, int index)
        {
            this.name = name;
        }
    }
}"
                },
                new object[]
                {
                    "With comments inside constructor body",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(int @::@index)
        {// comment1
            // comment2
        }   // comment3
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {// comment1
            this.index = index;
            // comment2
        }   // comment3
    }
}"
                },
                new object[]
                {
                    "With existing statement in constructor body",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly string name;
        public TestService(string name, int @::@index)
        {
            this.name = name;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly string name;
        private readonly int index;

        public TestService(string name, int index)
        {
            this.name = name;
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "When already initializing a field/property in another class",
                    @"
using System;

namespace HelloWorld
{
    class Dummy
    {
        public int Prop { get; set; }
    }

    class TestService
    {
        public TestService(int @::@index)
        {
            var d = new Dummy();
            d.Prop = index;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class Dummy
    {
        public int Prop { get; set; }
    }

    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {
            var d = new Dummy();
            d.Prop = index;
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Custom types",
                    @"
using System;

namespace HelloWorld
{
    interface IDummy
    {
        void Action();
    }

    class TestService
    {
        public TestService(IDummy @::@dummy)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    interface IDummy
    {
        void Action();
    }

    class TestService
    {
        private readonly IDummy dummy;

        public TestService(IDummy dummy)
        {
            this.dummy = dummy;
        }
    }
}"
                },
                new object[]
                {
                    "Field with the same name already exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public int index;
        public TestService(int @::@index)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public int index;
        private readonly int index1;

        public TestService(int index)
        {
            index1 = index;
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string input)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddInitializedFieldRefactoring(new SimpleNameGenerator());
            await provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside constructor name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public Test@::@Service(int index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Inside parameter type",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(i@::@nt index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Method parameter",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void Test(int @::@index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int m_index;
        public TestService(int @::@index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized (complex right expression)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;
        public TestService(int @::@index)
        {
            this.index = index * 2;
        }
    }
}"
                },
                new object[]
                {
                    "Non-empty span",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(int @:i:@ndex)
        {
        }
    }
}"
                }
            };
    }
}
