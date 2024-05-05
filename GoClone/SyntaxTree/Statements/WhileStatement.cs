using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class WhileStatement : IStatement
{
    public IExpression condition;
    public BlockStatement body;

    public static WhileStatement Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.While);
        var cond = Module.ParseExpression(reader);
        var body = BlockStatement.Parse(reader);
        return new WhileStatement { condition = cond, body = body };
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        var cond = context.function.AppendBasicBlock("cond");
        var body = context.function.AppendBasicBlock("loop");
        var exit = context.function.AppendBasicBlock("exit");
        builder.BuildBr(cond);

        builder.PositionAtEnd(cond);
        var conditionValue = condition.Emit(context, builder);
        builder.BuildCondBr(conditionValue, body, exit);

        builder.PositionAtEnd(body);
        this.body.Emit(context, builder);
        builder.BuildBr(cond);

        builder.PositionAtEnd(exit);
    }

    public void Resolve(IScope scope)
    {
        condition = condition.Resolve(scope);
        body.Resolve(scope);
    }

    public void Verify(IErrorHandler errorHandler)
    {
    }
}
