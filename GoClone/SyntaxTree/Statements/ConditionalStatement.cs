using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class ConditionalStatement : IStatement
{
    public IExpression condition;
    public BlockStatement thenCase;
    public BlockStatement? elseCase;

    public static ConditionalStatement Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.If);
        IExpression condition = Module.ParseExpression(reader);

        BlockStatement thenCase = BlockStatement.Parse(reader);

        BlockStatement? elseCase = null;
        if (reader.Next(TokenKind.Else))
        {
            elseCase = BlockStatement.Parse(reader);
        }

        return new ConditionalStatement
        {
            condition = condition,
            thenCase = thenCase,
            elseCase = elseCase,
        };
    }

    public void Resolve(IScope scope)
    {
        condition = condition.Resolve(scope);
        thenCase.Resolve(scope);
        elseCase?.Resolve(scope);
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        var fn = context.function;

        if (elseCase == null) 
        {
            var initialBlock = fn.LastBasicBlock;
            var cond = condition.Emit(context, builder);

            var thenBlock = fn.AppendBasicBlock("then");
            builder.PositionAtEnd(thenBlock);
            thenCase.Emit(context, builder);
            bool thenTerminates = thenBlock.LastInstruction.IsATerminatorInst.Handle != 0;

            var mergeBlock = fn.AppendBasicBlock("merge");

            builder.PositionAtEnd(initialBlock);
            builder.BuildCondBr(cond, thenBlock, mergeBlock);

            if (!thenTerminates)
            {
                builder.PositionAtEnd(thenBlock);
                builder.BuildBr(mergeBlock);
            }

            builder.PositionAtEnd(mergeBlock);
        }
        else
        {
            var initialBlock = fn.LastBasicBlock;
            var cond = condition.Emit(context, builder);

            // emit blocks

            var thenBlock = fn.AppendBasicBlock("then");
            builder.PositionAtEnd(thenBlock);
            thenCase.Emit(context, builder);
            bool thenTerminates = thenBlock.LastInstruction.IsATerminatorInst.Handle != 0;

            var elseBlock = fn.AppendBasicBlock("else");
            builder.PositionAtEnd(elseBlock);
            elseCase.Emit(context, builder);
            bool elseTerminates = elseBlock.LastInstruction.IsATerminatorInst.Handle != 0;

            builder.PositionAtEnd(initialBlock);
            builder.BuildCondBr(cond, thenBlock, elseBlock);

            var mergeBlock = fn.AppendBasicBlock("merge");
            if (!thenTerminates || !elseTerminates) 
            {
                if (!thenTerminates)
                {
                    builder.PositionAtEnd(thenBlock);
                    builder.BuildBr(mergeBlock);
                }
                if (!elseTerminates)
                {
                    builder.PositionAtEnd(elseBlock);
                    builder.BuildBr(mergeBlock);
                }
            }

            builder.PositionAtEnd(mergeBlock);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("if ");
        sb.Append(condition);
        sb.Append(' ');
        sb.Append(thenCase);
        if (elseCase is not null)
        {
            sb.Append(" else ");
            sb.Append(elseCase);
        }
        return sb.ToString();
    }
}
