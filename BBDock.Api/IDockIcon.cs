using System.Drawing;

namespace BBDock.Api
{
    public interface IDockIcon
    {
        void Render(Graphics graphics, float width, float height, bool isSelected);
    }
}
