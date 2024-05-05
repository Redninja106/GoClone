using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal interface IStatement
{
    void Resolve(IScope scope);
    void Verify(IErrorHandler errorHandler);
    void Emit(EmitContext context, LLVMBuilderRef builder);
}
