using System.Drawing;

namespace ProtoDock.Api
{
    public interface IDockIcon
    {
        IDockPanelMediator Mediator { get; }

        void Update();
        void Click();
        bool ContextClick();
        void Render(Graphics graphics, float width, float height, bool isSelected);
        bool Store(out string data);
    }
}
