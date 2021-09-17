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

        void MouseEnter();
        void MouseLeave();
        void MouseDown(int x, int y, MouseButtons button);
        bool MouseUp(int x, int y, MouseButtons button);
        void MouseMove(int x, int y, MouseButtons button);

        void Render(Graphics graphics, float width, float height, bool isSelected);        
        bool Store(out string data);
    }
}
