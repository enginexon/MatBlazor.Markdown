using System;
using System.Linq;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MatBlazor.Markdown.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

// ReSharper disable once CheckNamespace
namespace MatBlazor
{
    /// <summary>
    /// Component for rendering markdown text using MatBlazor typography
    /// </summary>
    public class MatMarkdown : BaseMatComponent
    {
        /// <summary>
        /// For internal usage, configuration for <see cref="Markdig.Markdown"/>
        /// </summary>
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        
        /// <summary>
        /// For internal usage during with <see cref="RenderTreeBuilder"/>
        /// </summary>
        private int _sequence = 0;

        /// <summary>
        /// Default tag for the markdown root element
        /// </summary>
        private const string MarkdownTag = "article";
        
        /// <summary>
        /// Default tag for a new line
        /// </summary>
        private const string NewLineTag = "br";
        
        /// <summary>
        /// Default tag for a code block
        /// </summary>
        private const string CodeTag = "code";
        
        /// <summary>
        /// Default tag for a paragraph block
        /// </summary>
        private const string ParagraphTag = "p";
        
        /// <summary>
        /// Markdown text
        /// </summary>
        [Parameter]
        public string Value { get; set; } = string.Empty;

        protected sealed override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (string.IsNullOrEmpty(Value)) return;
            
            var parsedText = Markdig.Markdown.Parse(Value, _pipeline);
            if (!parsedText.Any()) return;

            _sequence = 0;
            builder.OpenElement(_sequence++, MarkdownTag);
            BuildRenderTreeMarkdown(builder, parsedText);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdown(RenderTreeBuilder builder, ContainerBlock container)
        {
            foreach (var block in container)
            {
                switch (block)
                {
                    case ParagraphBlock paragraph:
                        BuildRenderTreeMarkdownParagraphBlock(builder, paragraph);
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

        private void BuildRenderTreeMarkdownParagraphBlock(RenderTreeBuilder builder, ParagraphBlock paragraph)
        {
            if (paragraph.Inline == null)  return;
            builder.OpenElement(_sequence++, ParagraphTag);
            builder.AddContent(_sequence++, (RenderFragment)(contentBuilder => BuildRenderTreeMarkdownInlines(contentBuilder, paragraph.Inline)));
            builder.CloseComponent();
        }

        private void BuildRenderTreeMarkdownInlines(RenderTreeBuilder builder, ContainerInline inlines)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case LiteralInline x:
                        BuildRenderTreeMarkdownLiteralInline(builder, x);
                        break;
                    case HtmlInline x:
                        BuildRenderTreeMarkdownHtmlInline(builder, x);
                        break;
                    case LineBreakInline x:
                        BuildRenderTreeMarkdownLineBreakInline(builder, x);
                        break;
                    case CodeInline x:
                        BuildRenderTreeMarkdownCodeInline(builder, x);
                        break;
                    case EmphasisInline x:
                        BuildRenderTreeMarkdownEmphasisInline(builder, x);
                        break;
                    case LinkInline x:
                        BuildRenderTreeMarkdownLinkInline(builder, x);
                        break;
                }
            }
        }

        private void BuildRenderTreeMarkdownLinkInline(RenderTreeBuilder builder, LinkInline linkInline)
        {
            var url = linkInline.Url;

            if (linkInline.IsImage)
            {
                var alt = linkInline
                    .OfType<LiteralInline>()
                    .Select(x => x.Content);

                builder.OpenElement(_sequence++, "img");
                builder.AddAttribute(_sequence++, "src", url);
                builder.AddAttribute(_sequence++, "alt", string.Join(string.Empty, alt));
                builder.CloseElement();
            }
            else
            {
                builder.OpenComponent<MatAnchorLink>(_sequence++);
                builder.AddAttribute(_sequence++, nameof(MatButtonLink.Href).ToLowerInvariant(), url); // MatAnchorLink has no href an attribute
                builder.AddAttribute(_sequence++, nameof(MatAnchorLink.ChildContent), (RenderFragment)(linkBuilder => BuildRenderTreeMarkdownInlines(linkBuilder, linkInline)));
                builder.CloseElement();
            }
        }

        private void BuildRenderTreeMarkdownEmphasisInline(RenderTreeBuilder builder, EmphasisInline emphasisInline)
        {
            if (!emphasisInline.TryGetEmphasisElement(out var elementEmphasisName)) return;

            builder.OpenElement(_sequence++, elementEmphasisName);
            BuildRenderTreeMarkdownInlines(builder, emphasisInline);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownCodeInline(RenderTreeBuilder builder, CodeInline codeInline)
        {
            builder.OpenElement(_sequence++, CodeTag);
            builder.AddContent(_sequence++, codeInline.Content);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownLineBreakInline(RenderTreeBuilder builder, LineBreakInline lineBreakInline)
        {
            builder.OpenElement(_sequence++, NewLineTag);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownHtmlInline(RenderTreeBuilder builder, HtmlInline htmlInline)
        {
            builder.AddMarkupContent(_sequence++, htmlInline.Tag);
        }

        private void BuildRenderTreeMarkdownLiteralInline(RenderTreeBuilder builder, LiteralInline literalInline)
        {
            builder.AddContent(_sequence++, literalInline.Content);
        }
    }
}