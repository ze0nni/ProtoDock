using System.Drawing;

namespace ProtoDock.Api
{
    public interface IDockIcon
    {
        IDockPanel Panel { get; }

        void Click();
        bool ContextClick();

        void Render(Graphics graphics, float width, float height, bool isSelected);
    }
}
