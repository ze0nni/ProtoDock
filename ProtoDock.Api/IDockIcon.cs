using System.Drawing;

namespace ProtoDock.Api
{
    public interface IDockIcon
    {
        IDockPanel Panel { get; }
        void Render(Graphics graphics, float width, float height, bool isSelected);
    }
}
