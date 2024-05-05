using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal interface IExpression
{
    IExpression Resolve(IScope scope);
    void Verify(IErrorHandler errorHandler) { }
    LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder);
    IType? GetResultType();
}
