using Markdig.Syntax.Inlines;

namespace MatBlazor.Markdown.Extensions
{
    internal static class EmphasisInlineExtensions
    {
        private const string ItalicsTag = "i";
        private const string BoldTag = "b";
        
        internal static bool TryGetEmphasisElement(this EmphasisInline emphasisInline, out string value)
        {
            value = emphasisInline.DelimiterChar switch
            {
                '*' => emphasisInline.DelimiterCount switch
                {
                    1 => ItalicsTag,
                    2 => BoldTag,
                    _ => ItalicsTag
                },
                '_' => emphasisInline.DelimiterCount switch
                {
                    1 => ItalicsTag,
                    2 => BoldTag,
                    _ => ItalicsTag
                },
                _ => string.Empty
            };

            return !string.IsNullOrEmpty(value);
        }
    }
}