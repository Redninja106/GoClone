using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Statements;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class BinaryExpression : IExpression
{
    public TokenKind op;
    public IExpression left;
    public IExpression right;

    public override string ToString()
    {
        bool leftParens = op is not TokenKind.Equal && left is BinaryExpression leftBin && Module.GetOperatorPrecedence(leftBin.op) > Module.GetOperatorPrecedence(op);
        bool rightParens = op is not TokenKind.Equal && right is BinaryExpression rightBin && Module.GetOperatorPrecedence(rightBin.op) > Module.GetOperatorPrecedence(op);

        string l = leftParens ? $"({left})" : left.ToString();
        string r = rightParens ? $"({right})" : right.ToString();

        return $"{l} {GetOperatorSymbol() ?? op.ToString()} {r}";
    }

    public string? GetOperatorSymbol()
    {
        return op switch
        {
            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.Equal => "=",
            TokenKind.GreaterThan => ">",
            TokenKind.GreaterThanEqual  => ">=",
            TokenKind.LessThan => "<",
            TokenKind.LessThanEqual  => "<=",
            TokenKind.EqualEqual => "==",
            TokenKind.NotEqual=> "!=",
            _ => null
        };
    }

    public IExpression Resolve(IScope scope)
    {
        left = left.Resolve(scope);
        right = right.Resolve(scope);

        if (op == TokenKind.Equal)
        {
            right = Module.ImplicitConvert(right, left.GetResultType());
        }

        if ((left.GetResultType() is not PrimitiveType p || p.PrimitiveKind != TokenKind.Int) && GetOperator() != null)
        {
            var op = scope.ResolveOperator(left.GetResultType()!, GetOperator()!.Value);
            return new CallExpression() 
            { 
                callee = new FunctionExpression()
                {
                    function = op,
                },
                arguments = [left, right],
            }.Resolve(scope);
        }

        return this;    
    }

    private IExpression? GetAssignableValue(IExpression expr)
    {
        if (expr is LocalVariableExpression var)
        {
            return var;
        }
        if (expr is MemberAccessExpression ma)
        {
            if (GetAssignableValue(ma.value) != null)
            {
                return ma;
            }
        }
        return null;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        if (op == TokenKind.Equal)
        {
            if (left is IAssignable assignable)
            {
                LLVMValueRef? assignableValue = assignable.EmitAssignablePointer(context, builder);
                if (assignableValue != null)
                {
                    var ptr = assignableValue.Value;
                    var value = right.Emit(context, builder);
                    builder.BuildStore(value, ptr);
                    return value;
                }
            }
            throw new("not assignable!");
        }

        var l = left.Emit(context, builder);
        var r = right.Emit(context, builder);
        
        switch (op)
        {
            case TokenKind.Plus:
                return builder.BuildAdd(l, r);
            case TokenKind.Minus:
                return builder.BuildSub(l, r);
            case TokenKind.Star:
                return builder.BuildMul(l, r);
            case TokenKind.Slash:
                return builder.BuildSDiv(l, r);
            case TokenKind.Percent:
                return builder.BuildSRem(l, r);
            case TokenKind.EqualEqual:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, l, r);
            case TokenKind.NotEqual:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, l, r);
            case TokenKind.LessThan:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, l, r);
            case TokenKind.LessThanEqual:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, l, r);
            case TokenKind.GreaterThan:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, l, r);
            case TokenKind.GreaterThanEqual:
                return builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, l, r);
            case TokenKind.OrOr:
                return builder.BuildOr(l, r);
            case TokenKind.AndAnd:
                return builder.BuildAnd(l, r);
            default:
                throw new();
        }
    }

    public OverloadableOperator? GetOperator()
    {
        return op switch
        {
            TokenKind.Plus => OverloadableOperator.Addition,
            TokenKind.Minus => OverloadableOperator.Subtraction,
            _ => null,
        };
    }

    public IType GetResultType()
    {

        switch (op)
        {
            case TokenKind.Plus:
            case TokenKind.Minus:
                return left.GetResultType();
            case TokenKind.Star:
            case TokenKind.Slash:
            case TokenKind.Percent:
                return new PrimitiveType { PrimitiveKind = TokenKind.Int };
            case TokenKind.EqualEqual:
            case TokenKind.NotEqual:
            case TokenKind.LessThan:
            case TokenKind.LessThanEqual:
            case TokenKind.GreaterThan:
            case TokenKind.GreaterThanEqual:
                return new PrimitiveType { PrimitiveKind = TokenKind.Bool };
            case TokenKind.Equal:
                return left.GetResultType();
            default:
                throw new("unknown op");
        }
    }
}
