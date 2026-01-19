using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Models;
using Windows.Foundation;

namespace TextControlBoxNS.Core.Text
{
    internal class LinkHighlightManager
    {
        public List<LinkInfo> links = new List<LinkInfo>();

        public bool HighlightLinks = true;

        private EventsManager eventsManager;
        private TextRenderer textRenderer;
        private CoreTextControlBox coreTextControlBox;
        private readonly Regex linkRegex = new Regex(@"(https?:\/\/[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Init(TextRenderer textRenderer, CoreTextControlBox coreTextControlBox, EventsManager eventsManager)
        {
            this.textRenderer = textRenderer;
            this.coreTextControlBox = coreTextControlBox;
            this.eventsManager = eventsManager;
        }

        public void FindAndComputeLinkPositions()
        {
            var matches = linkRegex.Matches(textRenderer.RenderedText);

            this.links.Clear();
            foreach (Match match in matches)
            {
                    var rects = textRenderer.DrawnTextLayout.GetCharacterRegions(match.Index, match.Length);
                Rect bounds = rects
                    .Select(r => r.LayoutBounds)
                    .Aggregate(Rect.Empty, (acc, r) => RectHelper.Union(acc, r));

                bounds.Y -= textRenderer.VerticalScrollOffset;
                
                this.links.Add(new LinkInfo
                {
                    StartIndex = match.Index,
                    Length = match.Length,
                    Url = match.Value,
                    Bounds = bounds
                });
            }     
        }

        public void CheckLinkHover(Point position)
        {
            bool isOverLink = false;
            foreach (var link in links)
            {
                if (link.Bounds.Contains(position))
                {
                    coreTextControlBox.ChangeCursor(Microsoft.UI.Input.InputSystemCursorShape.Hand);
                    isOverLink = true;
                    break;
                }
            }
            if (!isOverLink)
            {
                ResetCursorAfterHover();
            }
        }

        public void ResetCursorAfterHover()
        {
            coreTextControlBox.ChangeCursor(Microsoft.UI.Input.InputSystemCursorShape.IBeam);
        }

        public void CheckLinkClicked(Point position)
        {
            foreach (var link in links)
            {
                if (link.Bounds.Contains(position))
                {
                    eventsManager.CallLinkClicked(link.Url);
                    break;
                }
            }
        }

        public bool NeedsCheckLinkHighlights()
        {
            return HighlightLinks && links.Count > 0;
        }
    }
}
