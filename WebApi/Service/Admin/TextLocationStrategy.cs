using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using System.Collections.Generic;

namespace WebApi.Service.Admin
{
    public class TextLocationStrategy: ITextExtractionStrategy
    {
        public List<Rectangle> Locations = new();
        private readonly string _keyword;

        public TextLocationStrategy(string keyword)
        {
            _keyword = keyword;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;
            var renderInfo = (TextRenderInfo)data;
            string text = renderInfo.GetText();

            if (!string.IsNullOrEmpty(text) && text.Contains(_keyword))
            {
                Locations.Add(renderInfo.GetDescentLine().GetBoundingRectangle());
            }
        }

        public string GetResultantText() => "";
        public ICollection<EventType> GetSupportedEvents() => null;
    }
}
