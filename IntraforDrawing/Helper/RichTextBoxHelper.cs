using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace IntraforDrawing
{
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty BoundDocumentProperty =
            DependencyProperty.RegisterAttached(
                "BoundDocument", typeof(FlowDocument), typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundDocumentChanged));

        public static void SetBoundDocument(DependencyObject d, FlowDocument value) =>
            d.SetValue(BoundDocumentProperty, value);

        public static FlowDocument GetBoundDocument(DependencyObject d) =>
            (FlowDocument)d.GetValue(BoundDocumentProperty);

        private static void OnBoundDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox rtb && e.NewValue is FlowDocument doc)
            {
                rtb.Document = doc;
            }
        }
    }
}
