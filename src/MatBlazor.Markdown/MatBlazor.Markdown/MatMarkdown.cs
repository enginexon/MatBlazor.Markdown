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
        private const string MarkdownElementName = "article";
        
        /// <summary>
        /// Default tag for a new line
        /// </summary>
        private const string NewLineElementName = "br";
        
        /// <summary>
        /// Default tag for a code block
        /// </summary>
        private const string CodeElementName = "code";
        
        /// <summary>
        /// Default tag for a paragraph block
        /// </summary>
        private const string ParagraphElementName = "p";

        /// <summary>
        /// Default tag for a blockquote block
        /// </summary>
        private const string BlockquoteElementName = "blockquote";

        /// <summary>
        /// Default tag for an ordered list
        /// </summary>
        private const string OrderedListElementName = "ol";
        
        /// <summary>
        /// Default tag for an unordered list
        /// </summary>
        private const string UnorderedListElementName = "ul";

        /// <summary>
        /// Default tag for a list item
        /// </summary>
        private const string ListItemElementName = "li";

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
            builder.OpenElement(_sequence++, MarkdownElementName);
            BuildRenderTreeMarkdown(builder, parsedText);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdown(RenderTreeBuilder builder, ContainerBlock container)
        {
            foreach (var block in container)
            {
                switch (block)
                {
                    case ParagraphBlock paragraphBlock:
                        BuildRenderTreeMarkdownParagraphBlock(builder, paragraphBlock, null);
                        break;
                    case HeadingBlock headingBlock:
                        BuildRenderTreeMarkdownHeadingBlock(builder, headingBlock);
                        break;
                    case QuoteBlock quoteBlock:
                        BuildRenderTreeMarkdownQuoteBlock(builder, quoteBlock);
                        break;
                    case Table table:
                        BuildRenderTreeMarkdownTable(builder, table);
                        break;
                    case ListBlock listBlock:
                        BuildRenderTreeMarkdownListBlock(builder, listBlock);
                        break;
                    case ThematicBreakBlock thematicBreakBlock:
                        BuildRenderTreeMarkdownThematicBreakBlock(builder, thematicBreakBlock);
                        break;
                }
            }
        }

        private void BuildRenderTreeMarkdownThematicBreakBlock(RenderTreeBuilder builder, ThematicBreakBlock thematicBreakBlock)
        {
            builder.OpenComponent<MatDivider>(_sequence++);
            builder.CloseComponent();
        }

        private void BuildRenderTreeMarkdownListBlock(RenderTreeBuilder builder, ListBlock listBlock)
        {
            if (!listBlock.Any()) return;

            var elementName = listBlock.IsOrdered ? OrderedListElementName : UnorderedListElementName;
            builder.OpenElement(_sequence++, elementName);

            foreach (var blockItem in listBlock)
            {
                foreach (var blockItemInner in (ListItemBlock)blockItem)
                {
                    switch (blockItemInner)
                    {
                        case ListBlock listBlockInner:
                            BuildRenderTreeMarkdownListBlock(builder, listBlockInner);
                            break;
                        case ParagraphBlock paragraphBlock:
                            builder.OpenElement(_sequence++, ListItemElementName);
                            BuildRenderTreeMarkdownParagraphBlock(builder, paragraphBlock, null);
                            builder.CloseElement();
                            break;
                    }
                }
            }
            
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownTable(RenderTreeBuilder builder, Table table)
        {
            if (!table.Any()) return;
            
            // builder.OpenComponent<MatDataTable>(_sequence++);
            const string tableElementName = "table";
            builder.OpenElement(_sequence++, tableElementName);
            builder.AddAttribute(_sequence++, "class", "mdc-table");
            
            builder.AddContent(_sequence++, (RenderFragment)(tableBuilder =>
            {
                var header = table.First();
                var content = table.Skip(1);
                
                // thread
                const string headerElementName = "thead";
                const string headerCellElementName = "th";
                tableBuilder.OpenElement(_sequence++, headerElementName);
                BuildRenderTreeMarkdownTableRow(tableBuilder, (Markdig.Extensions.Tables.TableRow)header, headerCellElementName);
                tableBuilder.CloseElement();
                
                // tbody
                const string bodyElementName = "tbody";
                const string bodyCellElementName = "td";
                tableBuilder.OpenElement(_sequence++, bodyElementName);
                foreach (var row in content)
                {
                    BuildRenderTreeMarkdownTableRow(tableBuilder, (Markdig.Extensions.Tables.TableRow)row, bodyCellElementName);
                }
                tableBuilder.CloseElement();
            }));
            
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownTableRow(RenderTreeBuilder builder, Markdig.Extensions.Tables.TableRow tableRow, string cellElementName)
        {
            const string tableRowElementName = "tr";
            builder.OpenElement(_sequence++, tableRowElementName);
            builder.AddAttribute(_sequence++, "class", "mdc-table-header-row");
            builder.AddAttribute(_sequence++, "style", "white-space: nowrap;");

            foreach (var tableCell in tableRow.OfType<Markdig.Extensions.Tables.TableCell>())
            {
                builder.OpenElement(_sequence++, cellElementName);

                if (tableCell.Any() && tableCell.First() is ParagraphBlock paragraphBlock)
                {
                    BuildRenderTreeMarkdownParagraphBlock(builder, paragraphBlock, null);
                }
                
                builder.CloseElement();
            }
            
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownQuoteBlock(RenderTreeBuilder builder, QuoteBlock quoteBlock)
        {
            builder.OpenElement(_sequence++, BlockquoteElementName);
            BuildRenderTreeMarkdown(builder, quoteBlock);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownHeadingBlock(RenderTreeBuilder builder, HeadingBlock headingBlock)
        {
            if (headingBlock.Inline == null)  return;

            var matHeaderType = headingBlock.Level switch
            {
                1 => typeof(MatH1),
                2 => typeof(MatH2),
                3 => typeof(MatH3),
                4 => typeof(MatH4),
                5 => typeof(MatH5),
                6 => typeof(MatH6),
                _ => typeof(MatH6)
            };
            
            BuildRenderTreeMarkdownParagraphBlock(builder, headingBlock, matHeaderType);
        }

        private void BuildRenderTreeMarkdownParagraphBlock(RenderTreeBuilder builder, LeafBlock paragraph, Type matTypography)
        {
            if (paragraph.Inline == null)  return;
            if (matTypography != null)
            {
                builder.OpenComponent(_sequence++, matTypography);
                builder.AddAttribute(_sequence++, nameof(MatH1.ChildContent), (RenderFragment)(contentBuilder => BuildRenderTreeMarkdownInlines(contentBuilder, paragraph.Inline)));
                builder.CloseComponent();
            }
            else
            {
                builder.OpenElement(_sequence++, ParagraphElementName);
                builder.AddContent(_sequence++, (RenderFragment)(contentBuilder => BuildRenderTreeMarkdownInlines(contentBuilder, paragraph.Inline)));
                builder.CloseElement();
            }
        }

        private void BuildRenderTreeMarkdownInlines(RenderTreeBuilder builder, ContainerInline inlines)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case LiteralInline literalInline:
                        BuildRenderTreeMarkdownLiteralInline(builder, literalInline);
                        break;
                    case HtmlInline htmlInline:
                        BuildRenderTreeMarkdownHtmlInline(builder, htmlInline);
                        break;
                    case LineBreakInline lineBreakInline:
                        BuildRenderTreeMarkdownLineBreakInline(builder, lineBreakInline);
                        break;
                    case CodeInline codeInline:
                        BuildRenderTreeMarkdownCodeInline(builder, codeInline);
                        break;
                    case EmphasisInline emphasisInline:
                        BuildRenderTreeMarkdownEmphasisInline(builder, emphasisInline);
                        break;
                    case LinkInline linkInline:
                        BuildRenderTreeMarkdownLinkInline(builder, linkInline);
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

                const string imageElementName = "img";
                const string imageSrcAttr = "src";
                const string imageAltAttr = "alt";
                
                builder.OpenElement(_sequence++, imageElementName);
                builder.AddAttribute(_sequence++, imageSrcAttr, url);
                builder.AddAttribute(_sequence++, imageAltAttr, string.Join(string.Empty, alt));
                builder.CloseElement();
            }
            else
            {
                builder.OpenComponent<MatAnchorLink>(_sequence++);
                builder.AddAttribute(_sequence++, nameof(MatButtonLink.Href).ToLowerInvariant(), url); // MatAnchorLink has no href an attribute
                builder.AddAttribute(_sequence++, nameof(MatAnchorLink.ChildContent), (RenderFragment)(linkBuilder => BuildRenderTreeMarkdownInlines(linkBuilder, linkInline)));
                builder.CloseComponent();
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
            builder.OpenElement(_sequence++, CodeElementName);
            builder.AddContent(_sequence++, codeInline.Content);
            builder.CloseElement();
        }

        private void BuildRenderTreeMarkdownLineBreakInline(RenderTreeBuilder builder, LineBreakInline lineBreakInline)
        {
            builder.OpenElement(_sequence++, NewLineElementName);
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