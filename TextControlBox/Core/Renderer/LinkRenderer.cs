using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Core.Renderer
{
    internal class LinkRenderer
    {
        private TextRenderer textRenderer;
        private LinkHighlightManager linkHighlightManager;

        public void Init(TextRenderer textRenderer, LinkHighlightManager linkHighlightManager)
        {
            this.textRenderer = textRenderer;
            this.linkHighlightManager = linkHighlightManager;
        }

        public void HighlightLinks()
        {
            foreach (var link in linkHighlightManager.links)
            {
                textRenderer.DrawnTextLayout.SetUnderline(
                    link.StartIndex,
                    link.Length, true);
            }
        }
    }
}
