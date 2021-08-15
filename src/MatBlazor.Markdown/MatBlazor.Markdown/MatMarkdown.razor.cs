// ReSharper disable once CheckNamespace

using System;
using System.Linq;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MatBlazor
{
    public class MatMarkdown : BaseMatComponent
    {
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private int _sequence = 0;

        /// <summary>
        /// Markdown text to be rendered in the component.
        /// </summary>
        [Parameter]
        public string Value { get; set; } = string.Empty;

        protected sealed override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (string.IsNullOrEmpty(Value)) return;
            
            var parsedText = Markdig.Markdown.Parse(Value, _pipeline);
            if (parsedText.Count == 0) return;

            _sequence = 0;
            builder.OpenElement(_sequence++, "article");
            RenderMarkdown(parsedText, builder);
            builder.CloseElement();
        }

        private void RenderMarkdown(ContainerBlock container, RenderTreeBuilder builder)
        {
            foreach (var block in container)
            {
                switch (block)
                {
                    case ParagraphBlock paragraph:
                        RenderParagraphBlock(paragraph, builder);
                        break;
                    case HeadingBlock heading:
                        break;
                    case QuoteBlock quote:
                        break;
                    case Table table:
                        break;
                    case ListBlock list:
                        break;
                    case ThematicBreakBlock:
                        break;
                }
            }
        }

        private void RenderParagraphBlock(LeafBlock paragraph, RenderTreeBuilder builder)
        {
            if (paragraph.Inline == null)  return;
            builder.OpenElement(_sequence++, "p");
            builder.AddContent(_sequence++, (RenderFragment)(contentBuilder => RenderInlines(paragraph.Inline, contentBuilder)));
            builder.CloseComponent();
        }

        private void RenderInlines(ContainerInline inlines, RenderTreeBuilder builder)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case LiteralInline x:
                    {
                        builder.AddContent(_sequence++, x.Content);
                        break;
                    }
                    case HtmlInline x:
                    {
                        builder.AddMarkupContent(_sequence++, x.Tag);
                        break;
                    }
                    case LineBreakInline:
                    {
                        builder.OpenElement(_sequence++, "br");
                        builder.CloseElement();
                        break;
                    }
                    case CodeInline x:
                    {
                        builder.OpenElement(_sequence++, "code");
                        builder.AddContent(_sequence++, x.Content);
                        builder.CloseElement();
                        break;
                    }
                }
            }
        }
    }
}