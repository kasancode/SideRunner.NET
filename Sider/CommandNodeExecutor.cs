using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using Sider.Models;
using Sider.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sider
{
    public class CommandNodeExecutor : WebDriverExecutor
    {
        internal record CommandCondition(int Index, int Level, bool? Condition);

        public CommandNodeExecutor(IWebDriver driver, string baseUrl, Dictionary<string, object>? variables = null)
            : base(driver, baseUrl, variables)
        {
            this.controlStack = new();

        }

        Stack<CommandCondition> controlStack;
        int currentIndex = 0;
        int currentLevel = 0;
        bool skipping = false;

        internal void Reset()
        {
            this.controlStack.Clear();
            this.currentIndex = 0;
            this.currentLevel = 0;
            this.skipping = false;
        }

        public new void ExecuteTest(Test test)
        {
            var playback = CreatePlaybackTree(test.Commands, true) ?? throw new Exception("Incorrect test");

            var currentNode = playback.CommandNodes.First();

            while(currentNode != null)
            {
                var result = this.Execute(currentNode);
                currentNode = result.Next;
            }
        } 
  

        //https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/playback-tree/index.ts#L35
        internal PlaybackTree CreatePlaybackTree(List<Models.Command> commands, bool isValidationDisabled)
        {
            if (!isValidationDisabled)
            {
                validateControlFlowSyntax(commands);
            }
            var levels = deriveCommandLevels(commands);
            var nodes = createCommandNodes(commands, levels);
            nodes = connectCommandNodes(nodes);

            return new(nodes[0], nodes.ToArray(), nodes.Any(n => n.Command.IsControlFlow()));
        }

        private List<CommandNode> createCommandNodes(List<Models.Command> commands, List<int> levels)
            => commands
                .Zip(levels)
                .Select((item, i) => new CommandNode(item.First, item.Second, i))
                .ToList();


        private List<CommandNode> connectCommandNodes(List<CommandNode> nodes)
        {
            var states = new Stack<State>();
            for(var i = 0;i < nodes.Count-1; i++)
            {
                var node = nodes[i];
                var nextNode = nodes[i + 1];
                connectCommandNode(node, nextNode, nodes, states);
            }

            return nodes;
        }

        private void connectCommandNode(CommandNode node, CommandNode nextNode, List<CommandNode> nodes, Stack<State> states)
        {
            State? top;
            switch (node.Command.CommandName)
            {
                case ControlFlowCommandNames.Do:
                    states.Push(new(node.Command, node.Level, node.Index));
                    connectNext(node, nextNode);
                    break;

                case ControlFlowCommandNames.Else:
                    connectNext(node, nextNode);
                    break;

                case ControlFlowCommandNames.ElseIf:
                    connectConditional(node, nextNode, nodes);

                    break;

                case ControlFlowCommandNames.End:
                    if (!states.Any())
                        throw new Exception("Invalid end");

                    top = states.Pop();

                    if (top.Command.IsLoop() && nextNode.Command.IsEnd())
                    {
                        nextNode = nodes[top.Index];
                    }
                    connectNext(node, nextNode);
                    break;

                case ControlFlowCommandNames.ForEach:
                case ControlFlowCommandNames.If:
                case ControlFlowCommandNames.Times:
                case ControlFlowCommandNames.While:
                    states.Push(new(node.Command, node.Level, node.Index));
                    connectConditional(node, nextNode, nodes);
                    break;

                case ControlFlowCommandNames.RepeatIf:
                    if (!states.Any())
                        throw new Exception("Invalid repeat if");

                    top = states.Pop();
                    node.Right = nodes[top.Index];
                    node.Left = nextNode;
                    break;

                default:
                    if (states.Any())
                    {
                        top = states.Peek();

                        if (top.Command.IsIf() && nextNode.Command.IsElseOrElseIf())
                        {
                            nextNode = findNextNode(nodes, node.Index, top.Level, ControlFlowCommandNames.End) ?? throw new InvalidOperationException();
                        }
                        else if (top.Command.IsLoop() && nextNode.Command.IsEnd())
                        {
                            nextNode = nodes[top.Index];
                        }
                    }
                    connectNext(node, nextNode);
                    break;
            }
        }

        internal void connectConditional(CommandNode node, CommandNode nextNode, List<CommandNode> nodes)
        {
            node.Left = findNextNode(nodes, node.Index, node.Level);
            node.Right = nextNode;
        }

        internal CommandNode? findNextNode(List<CommandNode> stack, int index, int level, string? commandName = null)
        {
            for (var i = index + 1; i < stack.Count + 1; i++)
            {
                if (!string.IsNullOrEmpty(commandName))
                {
                    if (
                      stack[i].Level == level &&
                      stack[i].Command.CommandName == commandName
                    )
                    {
                        return stack[i];
                    }
                }
                else
                {
                    if (stack[i].Level == level)
                    {
                        return stack[i];
                    }
                }
            }
            return null;
        }

        internal void connectNext(CommandNode node, CommandNode nextNode)
        {
            node.Next = nextNode;
        }

        internal List<int> deriveCommandLevels(List<Models.Command> commands)
        {
            var level = 0;
            var levels = new List<int>();
            foreach (var command in commands)
            {
                level = levelCommand(command, level, levels);
            }
            return levels;
        }

        internal int levelCommand(Models.Command command, int level, List<int> levels)
        {
            if (command.Skip == true)
            {
                levels.Add(level);
                return level;
            }
            else
            {
                switch (command.CommandName)
                {
                    case ControlFlowCommandNames.Do:
                    case ControlFlowCommandNames.ForEach:
                    case ControlFlowCommandNames.If:
                    case ControlFlowCommandNames.Times:
                    case ControlFlowCommandNames.While:
                        levels.Add(level);
                        return level + 1;

                    case ControlFlowCommandNames.Else:
                    case ControlFlowCommandNames.ElseIf:
                        levels.Add(level - 1);
                        return level;

                    case ControlFlowCommandNames.End:
                    case ControlFlowCommandNames.RepeatIf:
                        level--;
                        levels.Add(level);
                        return level;

                    default:
                        levels.Add(level);
                        return level;
                }
            }
        }

        internal bool validateControlFlowSyntax(List<Models.Command> commands)
        {
            var state = new Stack<(Models.Command, int)>();
            for (var i = 0; i < commands.Count; i++)
            {
                this.validateCommand(commands[i], i, state);
            }
            return true;
        }

        private void validateCommand(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            switch (command.CommandName)
            {
                case ControlFlowCommandNames.Do:
                    trackControlFlowBranchOpen(command, index, state);
                    return;
                case ControlFlowCommandNames.Else:
                    varidateElse(command, index, state);
                    return;
                case ControlFlowCommandNames.ElseIf:
                    validateElseIf(command, index, state);
                    return;
                case ControlFlowCommandNames.End:
                    validateEnd(command, index, state);
                    return;
                case ControlFlowCommandNames.ForEach:
                    trackControlFlowBranchOpen(command, index, state);
                    return;
                case ControlFlowCommandNames.If:
                    trackControlFlowBranchOpen(command, index, state);
                    return;
                case ControlFlowCommandNames.RepeatIf:
                    validateRepeatIf(command, index, state);
                    return;
                case ControlFlowCommandNames.Times:
                    trackControlFlowBranchOpen(command, index, state);
                    return;
                case ControlFlowCommandNames.While:
                    trackControlFlowBranchOpen(command, index, state);
                    return;
            }
        }

        private void validateRepeatIf(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            var (topCommand, _) = state.Peek();

            if (!topCommand.IsDo())
            {
                throw new ControlFlowSyntaxException("A repeat if used without a do block", index);
            }
            state.Pop();
        }

        private void validateEnd(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            var (topCommand, _) = state.Peek();

            if (topCommand.IsBlockOpen())
            {
                state.Pop();
            }
            else if (topCommand.IsElseOrElseIf())
            {
                state.Pop();
                validateEnd(command, index, state);
            }
            else
            {
                throw new ControlFlowSyntaxException("Use of end without an opening keyword", index);
            }
        }

        private void validateElseIf(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            var (topCommand, topIndex) = state.Peek();
            if (!topCommand.IsIfBlock())
            {
                throw new ControlFlowSyntaxException("An else used outside of an if block", index);
            }
            if (topCommand.IsElse())
            {
                throw new ControlFlowSyntaxException("Incorrect command order of else if / else", index);
            }
            state.Push((command, index));
        }

        private void varidateElse(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            var (topCommand, topIndex) = state.Peek();
            if (!topCommand.IsIfBlock())
            {
                throw new ControlFlowSyntaxException("An else used outside of an if block", index);
            }
            if (topCommand.IsElse())
            {
                throw new ControlFlowSyntaxException("Too many else commands used", index);
            }
            state.Push((command, index));
        }

        private void trackControlFlowBranchOpen(Models.Command command, int index, Stack<(Models.Command, int)> state)
        {
            state.Push((command, index));
        }

        internal CommandExecutionResult Execute(CommandNode node)
        {
            if (node.Command.ShouldSkip())
            {
                return this.ExecutionResult(node, new(null, true, null));
            }
            else if (node.IsControlFlow)
            {
                return this.ExecutionResult(node, this.Evaluate(node));
            }
            else if (node.IsTerminal)
            {
                return this.ExecutionResult(node, new(null, false, null));
            }
            else
            {
                this.ExecuteCommand(node.Command);
                return this.ExecutionResult(node, null);
            }
        }

        internal CommandExecutionResult ExecutionResult(CommandNode node, CommandExecutionResult? result)
        {
            node.IncrementTimesVisited();
            return new(node.IsControlFlow && result is not null ? result.Next : node.Next, result?.Skiped ?? false, null);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/playback-tree/command-node.ts#L165
        internal CommandExecutionResult Evaluate(CommandNode node)
        {
            var (expression, args) = node.Command.Target.InterpolateScript(this.Variables);

            if (node.Command.IsTimes())
            {
                if (!double.TryParse(expression, out var number))
                {
                    throw new InvalidOperationException("Invalid number provided as a target.");
                }
                number = Math.Floor(number);
                return this.EvaluationResult(node, new(node.TimesVisited < number));
            }
            else if (node.Command.IsForEach())
            {
                var result = this.EvaluateForEach(node);
                return this.EvaluationResult(node, new(result));
            }

            var evalResult = EvaluateConditional(expression, args);
            return this.EvaluationResult(node, evalResult);
        }

        // https://github.com/SeleniumHQ/selenium-ide/blob/trunk/packages/side-runtime/src/playback-tree/command-node.ts#L140
        internal bool EvaluateForEach(CommandNode node)
        {
            var (script, _) = node.Command.Target.InterpolateScript(this.Variables);
            var collection = this.Variables[script];
            if (collection is not IEnumerable list) 
                throw new InvalidOperationException("Invalid variable provided.");

            var (varName, _) = node.Command.Value.InterpolateScript(this.Variables);

            var i = 0;
            foreach(var item in list)
            {
                if(i == node.TimesVisited)
                {
                    this.Variables[varName] = item;
                }
                i++;
            }

            var result = node.TimesVisited < i;
            if (!result)
            {
                node.TimesVisited = -1;
            }

            return result;
        }

        internal CommandExecutionResult EvaluationResult(CommandNode node, WebDriverExecutorCondEvalResult result)
        {
            if (result.Value)
            {
                return new(node.Right, false, null);
            }
            else
            {
                return new(node.Left, false, null);
            }
        }
    }
}
