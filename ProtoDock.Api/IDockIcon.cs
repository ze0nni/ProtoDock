using System.Drawing;

namespace ProtoDock.Api
{
    public interface IDockIcon
    {
        void Render(Graphics graphics, float width, float height, bool isSelected);
    }
}
