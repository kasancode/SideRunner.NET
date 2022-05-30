namespace Sider.Models
{
    public record CommandExecutionResult(CommandNode? Next, bool Skiped, object? Value);
}
