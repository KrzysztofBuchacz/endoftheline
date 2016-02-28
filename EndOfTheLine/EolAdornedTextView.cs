﻿using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace EndOfTheLine
{
    /// <summary>
    /// Adds and removes end of line adornments for the lines in a text view.
    /// </summary>
    public class EolAdornedTextView : IAdornedTextView<ITextViewLine>
    {
        private readonly IAdornmentLayer layer;
        private readonly IEolOptions eolOptions;

        public EolAdornedTextView(IWpfTextView view, IAdornmentLayer layer, IEolOptions eolOptions)
        {
            View = view;
            this.layer = layer;
            this.eolOptions = eolOptions;
        }

        public IWpfTextView View { get; }

        public Brush WhitespaceBrush { set; get; }

        public IList<ITextViewLine> Lines => View.TextViewLines;

        public void ClearAdornmentsFromLine(ITextViewLine line)
        {
            var lineBreak = new SnapshotSpan(View.TextSnapshot, Span.FromBounds(line.End, line.EndIncludingLineBreak));
            layer.RemoveAdornmentsByVisualSpan(lineBreak);
        }

        public void AddAdornmentToLine(ITextViewLine line)
        {
            var lineBreak = new SnapshotSpan(View.TextSnapshot, Span.FromBounds(line.End, line.EndIncludingLineBreak));
            var markerGeom = View.TextViewLines.GetMarkerGeometry(lineBreak);
            if (markerGeom == null)
            {
                return; 
            }

            var eolLabel = GetEolLabel(lineBreak.GetText());

            var textProp = View.FormattedLineSource.DefaultTextProperties;
            var typeface = textProp.Typeface;

            var textBlock = new TextBlock
            {
                Text = eolLabel,
                FontFamily = typeface.FontFamily,
                FontSize = textProp.FontRenderingEmSize,
                FontWeight = typeface.Weight,
                FontStretch = typeface.Stretch,
                FontStyle = typeface.Style,
                Foreground = WhitespaceBrush
            };

            UIElement adornment = textBlock;

            Canvas.SetLeft(adornment, markerGeom.Bounds.Left);
            Canvas.SetTop(adornment, markerGeom.Bounds.Top);

            layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, lineBreak, null, adornment, null);
        }

        public void CreateVisuals()
        {
            foreach (var line in View.TextViewLines)
            {
                AddAdornmentToLine(line);
            }
        }

        private string GetEolLabel(string lineBreakText)
        {
            if (!ShouldShowEnding(lineBreakText))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var c in lineBreakText)
            {
                switch (c)
                {
                    case '\r':
                        sb.Append("¤");
                        break;
                    case '\n':
                        sb.Append("¶");
                        break;
                    default:
                        sb.Append("<" + (int) c + ">");
                        break;
                }
            }

            return sb.ToString();
        }

        private bool ShouldShowEnding(string lineBreakText)
        {
            if (eolOptions.Visibility != VisibilityPolicy.WhenEndingIs)
            {
                return true;
            }

            switch (lineBreakText)
            {
                case "\r\n":
                    return eolOptions.WhenCrlf;
                case "\n":
                    return eolOptions.WhenLf;
                default:
                    return eolOptions.WhenOther;
            }
        }

        internal void RemoveAllAdornments()
        {
            layer.RemoveAllAdornments();
        }
    }
}