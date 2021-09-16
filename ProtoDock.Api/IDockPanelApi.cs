namespace ProtoDock.Api
{
    public interface IDockPanelApi
    {
        IDockApi Dock { get; }

        void Add(IDockIcon icon, bool playAppear);
        void Remove(IDockIcon icon, bool playDisappear);
        bool ScreenRect(IDockIcon icon, out System.Drawing.Rectangle rect);
    }
}
