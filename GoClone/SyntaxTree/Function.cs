using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Statements;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;

namespace GoClone.SyntaxTree;
internal class Function : IDeclaration, IResolvableValue
{
    public Token[] modifiers;
    public Parameter? receiver;
    public Token name;
    public List<Parameter> parameters = [];
    public IType? returnType;
    public BlockStatement? body;
    public OverloadableOperator? op;

    public LLVMBuilderRef varBuilder;
    public LLVMValueRef llvmFunction;
    public LLVMTypeRef llvmFunctionType;

    public LLVMValueRef? interfaceStub;

    public LLVMValueRef Value => llvmFunction;

    public static Function Parse(TokenReader reader, Token[] modifiers)
    {
        reader.NextOrError(TokenKind.Func);

        Parameter? receiver = null;
        if (reader.Next(TokenKind.OpenParenthesis))
        {
            receiver = Parameter.Parse(reader);
            reader.NextOrError(TokenKind.CloseParenthesis);
        }

        OverloadableOperator? op = null;
        Token name;
        if (reader.Next(TokenKind.Operator, out name))
        {
            name = reader.Current;
            op = ReadOperator(reader);
        }
        else
        {
            name = reader.NextOrError(TokenKind.Identifier);
        }
        List<Parameter> parameters = [];
        int index = 0;
        if (receiver != null)
        {
            index++;
        }

        if (reader.Next(TokenKind.OpenParenthesis) && !reader.Next(TokenKind.CloseParenthesis))
        {
            parameters.Add(Parameter.Parse(reader));
            while (!reader.Next(TokenKind.CloseParenthesis))
            {
                reader.NextOrError(TokenKind.Comma);
                parameters.Add(Parameter.Parse(reader));
            }
        }

        IType? returnType = null;
        if (reader.Next(TokenKind.Arrow))
        {
            returnType = Module.ParseType(reader);
        }

        BlockStatement? body = null;
        if (reader.Current.kind == TokenKind.OpenBrace)
        {
            body = BlockStatement.Parse(reader);
        }

        return new()
        {
            modifiers = modifiers,
            receiver = receiver,
            name = name,
            parameters = parameters,
            returnType = returnType,
            body = body,
            op = op,
        };
    }

    public FunctionType GetFunctionType()
    {
        IEnumerable<IType> parameters = this.parameters.Select(p => p.type);

        if (receiver is not null)
        {
            parameters = parameters.Prepend(receiver.type);
        }

        return new FunctionType { returnType = this.returnType, parameters = parameters.ToArray() };
    }

    public void EmitBody(ModuleScope scope)
    {
        if (body is null)
        {
            return;
        }

        var builder = scope.context.CreateBuilder();
        varBuilder = scope.context.CreateBuilder();
        var block = scope.context.AppendBasicBlock(llvmFunction, "entry");
        varBuilder.PositionAtEnd(block);
        builder.PositionAtEnd(block);

        if (receiver != null)
        {
            receiver.value = GetParameterValue(receiver);
        }
        foreach (var parameter in parameters)
        {
            parameter.value = GetParameterValue(parameter);
        }

        EmitContext ctx = new() { function = llvmFunction, llvmCtx = scope.context };
        body.Emit(ctx, builder);

        // defer var adding to the scope or smth so they are all in vars block

        foreach (var basicBlock in llvmFunction.BasicBlocks)
        {
            if (basicBlock.LastInstruction.IsATerminatorInst.Handle == 0)
            {
                if (returnType == null)
                {
                    builder.PositionAtEnd(basicBlock);
                    builder.BuildRetVoid();
                }
                else
                {
                    throw new Exception($"Function {name} does not return!");
                }
            }
        }
    }

    internal LLVMValueRef GetParameterValue(Parameter parameter)
    {
        if (parameter == receiver)
        {
            return llvmFunction.GetParam(0);
        }

        int baseIndex = receiver == null ? 0 : 1;
        return llvmFunction.GetParam((uint)(baseIndex + parameters.IndexOf(parameter)));
    }

    public void Emit(ModuleScope scope)
    {
        LLVMTypeRef returnType = this.returnType?.Emit(scope.context) ?? scope.context.VoidType;
        
        IEnumerable<Parameter> signature = parameters;
        if (receiver is not null)
            signature = signature.Prepend(receiver);

        LLVMTypeRef[] parameterTypes = signature.Select(p => p.type.Emit(scope.context)).ToArray();

        llvmFunctionType = LLVMTypeRef.CreateFunction(returnType, parameterTypes);
        if (llvmFunctionType.Context != scope.context)
            throw new Exception();
        llvmFunction = scope.module.AddFunction(GetLLVMName(), llvmFunctionType);

        // create interface stub
        // interface implementation functions must accept a pointer to the type they are implementing for
        // so if we accept the direct value, we generate a extra function that accepts a pointer and calls this
        // one with its value. We call this the interface stub. This function's interface stub is then used in
        // place of this function when generating vtables. 
        if (receiver is not null && receiver.type is not PointerType)
        {
            var stubReceiverType = new PointerType { elementType = receiver.type };
            parameterTypes[0] = stubReceiverType.Emit(scope.context);
            var stubFunctionType = LLVMTypeRef.CreateFunction(returnType, parameterTypes);
            var stub = scope.module.AddFunction(GetLLVMName() + ".istub", stubFunctionType);

            // we can emit the stubs body now: it doesn't need to reference anything except the main function

            using LLVMBuilderRef builder = scope.context.CreateBuilder();
            var block = stub.AppendBasicBlock("entry");
            builder.PositionAtEnd(block);

            var valuePtr = stub.FirstParam;
            var value = builder.BuildLoad2(receiver.type.Emit(scope.context), valuePtr);
            var call = builder.BuildCall2(llvmFunctionType, llvmFunction, [value, .. stub.Params.Skip(1)]);
            if (this.returnType == null)
            {
                builder.BuildRetVoid();
            } 
            else 
            {
                builder.BuildRet(call);
            }
            interfaceStub = stub;
        }
    }

    private static OverloadableOperator ReadOperator(TokenReader reader)
    {
        if (reader.Next(TokenKind.Plus))
        {
            return OverloadableOperator.Addition;
        }
        if (reader.Next(TokenKind.Minus))
        {
            return OverloadableOperator.Subtraction;
        }
        if (reader.Next(TokenKind.OpenBracket))
        {
            reader.NextOrError(TokenKind.CloseBracket);
            return OverloadableOperator.Indexing;
        }
        throw new Exception("Expected an operator!");
    }

    public string GetLLVMName()
    {
        var result = $"{name}";

        if (receiver != null)
            result = $"{receiver.type.GetPointerElementType()}.{result}";

        return result;
    }

    public override string ToString()
    {
        StringBuilder result = new();
        result.Append("func ");
        if (receiver is not null)
        {
            result.Append('(');
            result.Append(receiver);
            result.Append(") ");
        }

        result.Append(name);
        result.Append('(');
        foreach (var parameter in parameters)
        {
            if (parameter != parameters[0])
            {
                result.Append(", ");
            }
            result.Append(parameter.ToString());
        }
        result.Append(')');

        if (returnType != null)
        {
            result.Append(" -> ");
            result.Append(returnType.ToString());
        }

        if (body != null)
        {
            result.Append(' ');
            result.Append(body);
        }

        return result.ToString();
    }

    public string GetName()
    {
        return name.ToString();
    }

    public void Register(ModuleScope scope)
    {
        if (receiver is null)
        {
            scope.valueDeclarations.Add(name.ToString(), this);
        }
        else if (this.op != null)
        {
            receiver.type = receiver.type.Resolve(scope);
            if (!scope.operators.TryGetValue(receiver.type.GetElementType(), out var map))
            {
                scope.operators.Add(receiver.type.GetElementType(), map = []);
            }

            map.Add(this.op.Value, this);
        }
        else
        {
            receiver.type = receiver.type.Resolve(scope);
            if (!scope.receivers.TryGetValue(receiver.type.GetElementType(), out var map))
            {
                scope.receivers.Add(receiver.type.GetElementType(), map = []);
            }

            map.Add(this.name.ToString(), this);
        }
    }

    public void Resolve(ModuleScope scope)
    {
        FunctionScope functionScope = new(scope, this);
        if (receiver != null)
        {
            receiver.type = receiver.type.Resolve(scope);
        }
        returnType = returnType?.Resolve(scope);
        foreach (var param in parameters)
        {
            param.type = param.type.Resolve(scope);
        }
        body?.Resolve(functionScope);
    }

    public void Verify(ModuleScope scope)
    {
        body?.Verify(scope.errorHandler);
    }
}

class Parameter : IResolvableValue, IEquatable<Parameter>
{
    public IType type;
    public Token name;

    public LLVMValueRef value;
    public LLVMValueRef Value => value;

    public static Parameter Parse(TokenReader reader)
    {
        IType type = Module.ParseType(reader);
        Token name = reader.NextOrError(TokenKind.Identifier);
        return new Parameter { type = type, name = name };
    }

    public bool Equals(Parameter? other)
    {
        return type.Equals(other.type) && name.Value.SequenceEqual(other.name.Value);
    }

    public override string ToString()
    {
        return $"{type} {name}";
    }

    
}

enum OverloadableOperator
{
    Addition,
    Subtraction,
    Indexing,
}