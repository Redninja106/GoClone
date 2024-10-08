﻿using GoClone.SyntaxTree;
using GoClone.SyntaxTree.Expressions;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.CodeGeneration;
internal interface IScope
{
    ModuleScope GetModule();
    FunctionScope GetFunction();

    LLVMValueRef GetLLVMFunction()
    {
        return GetFunction().function.llvmFunction;
    }

    IResolvableType ResolveType(Token identifier);
    IResolvableValue ResolveValue(Token identifier);

    Function ResolveReceiver(IType type, Token name);
    Function ResolveOperator(IType type, OverloadableOperator op);
    LLVMValueRef GetInterfaceVTable(IType type, IType iface);

    // value -> interface
    // interface -> interface
    // interface -> ... -> interface
    // value -> interface -> interface -> ... -> interface
    IType MakeImplicitConversion(IExpression expression)
    {
        throw null;
    }
}
