using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.Constructor;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.Constructor
{
    public class AddConstructorParameterRefactoringTests : RefactoringTest
    {
        public AddConstructorParameterRefactoringTests(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider =>
            new AddConstructorParameterRefactoring(new OptionsService());

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) => 
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Constructor(s) exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int @::@index;

        public TestService()
        {
        }

        public TestService(string name)
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

        public TestService(string name, int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized in one constructor (multiple constructors)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        private readonly string name;

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int m_index;
        private readonly string name;

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Parameter with the same name exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService(string index)
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
        private int index;

        public TestService(string index, int index1)
        {
            this.index = index1;
        }
    }
}"
                },
                new object[]
                {
                    "Parameter with the same type exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService(int p)
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
        private int index;

        public TestService(int p)
        {
            index = p;
        }
    }
}"
                },
                new object[]
                {
                    "Parameter with the same type exists but initializes something else",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;
        private int p;

        public TestService(int p)
        {
            this.p = p;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;
        private int p;

        public TestService(int p, int index)
        {
            this.p = p;
            this.index = index;
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
                    "Outside field name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly i@::@nt index;
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
        private readonly int in@:d:@ex;
    }
}"
                },
                new object[]
                {
                    "Field already initialized (single constructor)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        public TestService(int index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized in a parameterless constructor",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;

        public TestService()
        {
            m_index = 2;
        }
    }
}"
                },
                new object[]
                {
                    "Field initialized from another constructor with other parameters",
                    @"
using System;

namespace HelloWorld
{
    class Test { public int Prop { get { return 2; } } }
    class TestService
    {
        private int @::@index;

        public TestService(Test test)
        {
            this.index = test.Prop;
        }
    }
}"
                },
                new object[]
                {
                    "Field is a constant",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private const int @::@index = 1;

        public TestService()
        {
        }
    }
}"
                },
                new object[]
                {
                    "Static constructor",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index = 1;

        static TestService()
        {
        }
    }
}"
                }
            };
    }
}
