using Sider.Models;
using System;

namespace Sider.Services
{
    public static class ControlFlowCommandNames
    {
        public const string Do = "do";
        public const string Else = "else";
        public const string ElseIf = "elseIf";
        public const string End = "end";
        public const string ForEach = "forEach";
        public const string If = "if";
        public const string RepeatIf = "repeatIf";
        public const string Times = "times";
        public const string While = "while";
    }

    public static class ControlFlowCommandChecks
    {

        internal static bool IsConditional(this Command command)
        {
            if (!command.IsCommandEnabled()) return false;

            return command.CommandName switch
            {
                ControlFlowCommandNames.ElseIf or
                ControlFlowCommandNames.If or
                ControlFlowCommandNames.RepeatIf or
                ControlFlowCommandNames.Times or
                ControlFlowCommandNames.While => true,
                _ => false,
            };
        }

        internal static bool IsControlFlow(this Command command)
        {
            if (!command.IsCommandEnabled()) return false;

            return command.CommandName switch
            {
                ControlFlowCommandNames.If or
                ControlFlowCommandNames.ElseIf or
                ControlFlowCommandNames.Else or
                ControlFlowCommandNames.End or
                ControlFlowCommandNames.Do or
                ControlFlowCommandNames.RepeatIf or
                ControlFlowCommandNames.Times or
                ControlFlowCommandNames.While => true,
                _ => false,
            };
        }

        internal static bool IsTerminal(this Command command)
            => IsElse(command) || IsDo(command) || IsEnd(command);

        internal static bool IsEnd(this Command command)
            => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.End);

        internal static bool IsDo(this Command command)
            => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.Do);

        internal static bool IsElse(this Command command)
            => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.Else);

        internal static bool IsIf(this Command command)
            => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.If);

        internal static bool IsBlockOpen(this Command command)
            => IsIf(command) || IsLoop(command);

        internal static bool IsIfBlock(this Command command)
            => IsIf(command) || IsElseOrElseIf(command);

        internal static bool IsElseOrElseIf(this Command command)
            => IsElse(command) || IsElseIf(command);


        internal static bool IsElseIf(this Command command)
            => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.ElseIf);

        internal static bool IsTimes(this Command command)
             => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.Times);

        internal static bool IsForEach(this Command command)
             => IsCommandEnabled(command)
            && CommandNamesEqual(command, ControlFlowCommandNames.ForEach);

        private static bool CommandNamesEqual(this Command command, string target)
            => command.CommandName == target;


        internal static bool IsCommandEnabled(this Command? command)
            => command is not null && !(command.Skip == true);

        internal static bool IsLoop(this Command command)
            => IsCommandEnabled(command) && (
                CommandNamesEqual(command, ControlFlowCommandNames.While) ||
                CommandNamesEqual(command, ControlFlowCommandNames.Times) ||
                CommandNamesEqual(command, ControlFlowCommandNames.RepeatIf) ||
                CommandNamesEqual(command, ControlFlowCommandNames.ForEach));

        internal static bool ShouldSkip(this Command command)
            => command.Skip == true || command.CommandName.StartsWith("//");
    }
}
