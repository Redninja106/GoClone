using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class TypeDeclaration : IDeclaration, IResolvableType
{
    public Token name;
    public IType target;

    public LLVMTypeRef Type { get; private set; }

    public static TypeDeclaration Parse(TokenReader reader)
    {
        if (reader.Next(TokenKind.Type))
        {
            Token name = reader.NextOrError(TokenKind.Identifier);
            reader.NextOrError(TokenKind.As);
            IType target = Module.ParseType(reader);
            return new TypeDeclaration { name = name, target = target };
        }
        if (reader.Next(TokenKind.Interface))
        {
            Token name = reader.NextOrError(TokenKind.Identifier);
            IType target = InterfaceType.Parse(reader, true);
            return new TypeDeclaration { name = name, target = target };
        }
        if (reader.Next(TokenKind.Struct))
        {
            Token name = reader.NextOrError(TokenKind.Identifier);
            IType target = StructType.Parse(reader, true);
            return new TypeDeclaration { name = name, target = target };
        }
        throw new();
    }

    public void Emit(ModuleScope scope)
    {
        if (target is StructType structType)
        {
            Type.StructSetBody(
                structType.fields.Select(f => f.type.Emit(scope.context)).ToArray(),
                false
                );
        }
    }

    public string GetName()
    {
        return name.ToString();
    }

    public void Register(ModuleScope scope)
    {
        scope.typeDeclarations.Add(name.ToString(), this);
        if (target is StructType)
        {
            Type = scope.context.CreateNamedStruct(name.Value);
        }
        else
        {
            Type = target.Emit(scope.context);
        }
    }

    public void Resolve(ModuleScope scope)
    {
        target = target.Resolve(scope);
    }

    public override string ToString()
    {
        return $"type {name} {target}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(name.Value.ToString(), target.GetHashCode());
    }

    public void Verify(ModuleScope scope)
    {
    }
}
