using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sider.Models
{
    public record PlaybackTree(CommandNode StartingCommandNode, CommandNode[] CommandNodes, bool ContainsControlFlow);
}
