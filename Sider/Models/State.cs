using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sider.Models
{
    internal record State
    (
        Command Command,
        int Level,
        int Index
    );
}
