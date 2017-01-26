using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.Xunit;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.Xunit
{
    public class ScaffoldXunitTheoryInlineDataRefactoringTests : RefactoringTest
    {
        public ScaffoldXunitTheoryInlineDataRefactoringTests(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider =>
            new ScaffoldXunitTheoryInlineDataRefactoring(new OptionsService());

        protected override IEnumerable<MetadataReference> AdditionalReferences =>
            new[] { MetadataReference.CreateFromFile(typeof(FactAttribute).Assembly.Location) };

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "No InlineData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [The@::@ory]
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Empty InlineData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Empty InlineData - Separate Attribute List",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]@::@
        [InlineData]
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Missing Theory",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [InlineData]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (single types)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", 5, 0.5, 0.5f, 0.5M, true, false, null)]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", 5, 0.5, 0.5f, 0.5M, true, false, null)]
        public TestMethod(string param1, int param2, double param3, float param4, decimal param5, bool param6, bool param7, string param8)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (implicit arrays)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new[] { ""value1"" }, new[] { 5 }, new[] { 0.5 }, new[] { 0.5f }, new[] { 0.5M }, new[] { true })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new[] { ""value1"" }, new[] { 5 }, new[] { 0.5 }, new[] { 0.5f }, new[] { 0.5M }, new[] { true })]
        public TestMethod(string[] param1, int[] param2, double[] param3, float[] param4, decimal[] param5, bool[] param6)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (explicit arrays)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new string[] { ""value1"" }, new int[] { 5 }, new double[] { 0.5 }, new float[] { 0.5f }, new decimal[] { 0.5M }, new bool[] { true }, new long[] { 1 }, new short[] { 2 })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new string[] { ""value1"" }, new int[] { 5 }, new double[] { 0.5 }, new float[] { 0.5f }, new decimal[] { 0.5M }, new bool[] { true }, new long[] { 1 }, new short[] { 2 })]
        public TestMethod(string[] param1, int[] param2, double[] param3, float[] param4, decimal[] param5, bool[] param6, long[] param7, short[] param8)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (explicit arrays, unsigned)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new uint[] { 5 }, new ulong[] { 1 }, new ushort[] { 2 })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new uint[] { 5 }, new ulong[] { 1 }, new ushort[] { 2 })]
        public TestMethod(uint[] param1, ulong[] param2, ushort[] param3)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (explicit arrays, full type names)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new System.Int32[] { 5 }, new System.UInt64[] { 1 })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(new System.Int32[] { 5 }, new System.UInt64[] { 1 })]
        public TestMethod(int[] param1, ulong[] param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (System.Type)",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(typeof(TestService), new[] { typeof(TestService) })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(typeof(TestService), new[] { typeof(TestService) })]
        public TestMethod(System.Type param1, System.Type[] param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Not empty InlineData (System.Type, with simplification)",
                    @"using System;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(typeof(TestService), new[] { typeof(TestService) }, new Type[] { typeof(TestService) })]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"using System;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(typeof(TestService), new[] { typeof(TestService) }, new Type[] { typeof(TestService) })]
        public TestMethod(Type param1, Type[] param2, Type[] param3)
        {
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
                    "Theory attribute not classified",
                    @"
namespace HelloWorld
{
    class TestService
    {
        [The@::@ory]
        public TestMethod()
        {
        }
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - method parameters exist",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData]@::@
        public TestMethod(string param1, int param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - alternate test data attribute",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData]@::@
        public TestMethod()
        {
        }
    }
}"
                }
            };
    }
}
