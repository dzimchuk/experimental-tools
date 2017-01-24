using ExperimentalTools.Tests.Infrastructure.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Features.Braces;
using ExperimentalTools.Services;
using Xunit;

namespace ExperimentalTools.Tests.Features.Braces
{
    public class AddBracesRefactoringTests : RefactoringTest
    {
        public AddBracesRefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => new AddBracesRefactoring(new OptionsService());

        [Theory, MemberData("HasActionTestData")]
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
                    "Inside parent statement",
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
            };
    }
}
