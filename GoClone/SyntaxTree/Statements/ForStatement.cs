using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class ForStatement : IStatement
{
    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        throw new NotImplementedException();
    }

    public void Resolve(IScope scope)
    {
        throw new NotImplementedException();
    }

    public void Verify(IErrorHandler errorHandler)
    {
        throw new NotImplementedException();
    }
}
