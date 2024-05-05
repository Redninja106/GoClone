using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class InterfaceType : IType
{
    public List<Function> functions;

    public static InterfaceType Parse(TokenReader reader, bool skipKeyword = false)
    {
        if (!skipKeyword)
        {
            reader.NextOrError(TokenKind.Interface);
        }
        reader.NextOrError(TokenKind.OpenBrace);

        List<Function> functions = new();

        while (!reader.Next(TokenKind.CloseBrace))
        {
            functions.Add(Function.Parse(reader, []));
            if (reader.Next(TokenKind.CloseBrace))
            {
                break;
            }
            reader.NextOrError(TokenKind.Comma);
        }

        return new InterfaceType { functions = functions };
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return default;
        throw new Exception("cannot use an interface here as it is a non-concrete type");
    }

    public bool Equals(IType? other)
    {
        return other is InterfaceType interfaceType && functions.SequenceEqual(interfaceType.functions);
    }

    public IType Resolve(IScope scope)
    {
        foreach (var fn in functions)
        {
            fn.Resolve(scope.GetModule());
        }
        return this;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("interface { ");
        foreach (var function in functions)
        {
            sb.Append(function);
            if (function != functions[^1])
                sb.Append(", ");
        }

        sb.Append(" }");
        return sb.ToString();
    }
}
