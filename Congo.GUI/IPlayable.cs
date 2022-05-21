using Congo.Core;

namespace Congo.GUI
{
    internal interface IPlayable
    {
        CongoGame Game { get; }
        CongoUser White { get; }
        CongoUser Black { get; }
    }
}
