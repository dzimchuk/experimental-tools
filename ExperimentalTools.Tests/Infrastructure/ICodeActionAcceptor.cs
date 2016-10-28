using Microsoft.CodeAnalysis.CodeActions;

namespace ExperimentalTools.Tests.Infrastructure
{
    internal interface ICodeActionAcceptor
    {
        void Accept(CodeAction action);
    }
}