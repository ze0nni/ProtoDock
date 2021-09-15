using System.Drawing;
using System.Windows.Forms;

namespace ProtoDock.Api
{
    public interface IDockIcon
    {
        IDockPanelMediator Mediator { get; }

        string Title { get; }
        float Width { get; }
        bool Hovered { get; }

        void Update();
        void Click();
        bool ContextClick();
        void Render(Graphics graphics, float width, float height, bool isSelected);        
        bool Store(out string data);
    }
}
