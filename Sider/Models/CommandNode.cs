using Sider.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sider.Models
{

    public class CommandNode
    {
        internal Command Command { get; set; }

        internal CommandNode? Next { get;set; }
        internal CommandNode? Left { get; set; }
        internal CommandNode? Right { get; set; }
        internal int Index { get; set; }
        internal int Level { get; set; }
        internal int TimesVisited { get; set; }

        internal CommandNode(Command command)
        {
            this.Command = command;
        }

        internal CommandNode(Command command, int level, int index)
        {
            this.Command = command;
            this.Level = level;
            this.Index = index;
        }

        internal bool IsControlFlow => this.Left != null || this.Right != null;

        internal bool IsTerminal
            => this.Command.IsTerminal()
            || this.Command.CommandName == "";

        internal void IncrementTimesVisited()
        {
            if (this.Command.IsLoop())
                this.TimesVisited++;
        }
    }
}
