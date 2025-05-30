using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;

namespace WebApi.Service.Admin
{
    public class GeneralTextLocationStrategy : IEventListener
    {

        public class TextLocation
        {
            public string Text { get; set; }
            public Rectangle Rect { get; set; }
        }

        public List<TextLocation> Locations { get; } = new();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;

            var renderInfo = (TextRenderInfo)data;
            string text = renderInfo.GetText();

            if (!string.IsNullOrWhiteSpace(text))
            {
                var rect = renderInfo.GetBaseline().GetBoundingRectangle();
                Locations.Add(new TextLocation
                {
                    Text = text,
                    Rect = new Rectangle((float)rect.GetX(), (float)rect.GetY(), (float)rect.GetWidth(), (float)rect.GetHeight())
                });
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_TEXT };
        }
    }
}
