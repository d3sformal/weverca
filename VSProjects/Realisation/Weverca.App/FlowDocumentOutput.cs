using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Weverca.Output.Output;

namespace Weverca.App
{
    class FlowDocumentOutput : OutputBase
    {
        private FlowDocument document;
        private Paragraph currentParagraph;
        private int paragraphLeftMargin = 0;

        public FlowDocumentOutput(FlowDocument document)
        {
            this.document = document;
        }

        public void ClearDocument()
        {
            document.Blocks.Clear();

            newParagraph();
        }

        private void newParagraph()
        {
            currentParagraph = new Paragraph();
            currentParagraph.Margin = new System.Windows.Thickness(paragraphLeftMargin, 0, 0, 0);

            document.Blocks.Add(currentParagraph);
        }


        protected override void head(string text)
        {
            Span header = new Span();
            header.FontWeight = FontWeights.Bold;
            header.FontSize = 16;
            header.Inlines.Add(text);

            currentParagraph.Inlines.Add(header);
        }

        protected override void head2(string text)
        {
            Span header = new Span();
            header.FontWeight = FontWeights.Bold;
            header.FontSize = 12;
            header.TextDecorations.Add(TextDecorations.Underline);
            header.Inlines.Add(text);

            currentParagraph.Inlines.Add(header);
        }

        protected override void info(string text)
        {
            currentParagraph.Inlines.Add(text);
        }

        protected override void hint(string text)
        {
            Span span = new Span();
            span.Foreground = Brushes.DarkGray;
            span.Inlines.Add(text);

            currentParagraph.Inlines.Add(span);
        }

        protected override void comment(string text)
        {
            currentParagraph.Inlines.Add(text);
        }

        protected override void delimiter(string text)
        {
            currentParagraph.Inlines.Add(text);
        }

        protected override void variable(string name)
        {
            Span span = new Span();
            span.Foreground = Brushes.Blue;
            span.FontWeight = FontWeights.Bold;
            span.Inlines.Add(name);

            currentParagraph.Inlines.Add(span);
        }

        protected override void error(string error)
        {
            Span span = new Span();
            span.Foreground = Brushes.DarkRed;
            span.FontWeight = FontWeights.Bold;
            span.Inlines.Add(error);

            currentParagraph.Inlines.Add(span);
        }

        protected override void line()
        {
            newParagraph();
        }

        protected override void setIndentation(int level)
        {
            this.paragraphLeftMargin = level * 10;
        }
    }
}
