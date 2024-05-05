using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class StructType : IType
{
    public List<Parameter> fields;

    public static StructType Parse(TokenReader reader, bool skipKeyword = false)
    {
        if (!skipKeyword)
        {
            reader.NextOrError(TokenKind.Struct);
        }
        reader.NextOrError(TokenKind.OpenBrace);

        List<Parameter> fields = new();

        while (!reader.Next(TokenKind.CloseBrace))
        {
            fields.Add(Parameter.Parse(reader));
            if (reader.Next(TokenKind.CloseBrace))
            {
                break;
            }
            reader.NextOrError(TokenKind.Comma);
        }

        return new StructType { fields = fields };
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return context.GetStructType(fields.Select(f => f.type.Emit(context)).ToArray(), false);
    }

    public bool Equals(IType? other)
    {
        return other is StructType str && str.fields.SequenceEqual(this.fields);
    }

    public IType Resolve(IScope scope)
    {
        foreach (var field in fields)
        {
            field.type = field.type.Resolve(scope);
        }
        return this;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("struct { ");
        foreach (var field in fields)
        {
            sb.Append(field.type);
            sb.Append(' ');
            sb.Append(field.name);
            if (field != fields[^1])
                sb.Append(", ");
        }

        sb.Append(" }");
        return sb.ToString();
    }
}
