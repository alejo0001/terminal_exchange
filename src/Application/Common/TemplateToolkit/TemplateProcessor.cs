using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmAPI.Application.Common.TemplateToolkit;

/// <summary>
///     A performant tool for bulk string replacements.
/// </summary>
/// <remarks>
///     <para>
///         Relies on <see cref="StringBuilder" /> to avoid repeated string creations which reduces GC pressure.
///     </para>
///     <para>
///         Walk-through of the template string is done only once and variable comparisons are internally done using
///         <b>Trie</b> data structure. In terms of memory it's costly, but this structure is created only once for
///         replacement operation.
///     </para>
///     <para>
///         Hopefully this tool has sufficient balance between performance and overall memory consumption.
///     </para>
/// </remarks>
/// <seealso href="https://www.geeksforgeeks.org/introduction-to-trie-data-structure-and-algorithm-tutorials/" />
public static class TemplateProcessor
{
    /// <summary>
    ///     Cleans up replacement values from excess spaces and then replaces variables in template.
    /// </summary>
    /// <param name="template"></param>
    /// <param name="ignoreCase">
    ///     Defaults to ignore variable case sensitivity to be more unforgiving to user who manages
    ///     templating. On the flip-side it means that variable structure must be a bit more solid, like having more
    ///     complex boundary markers to distinguish variable from normal text with higher level of certainty.
    /// </param>
    /// <param name="replacements"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         This overload uses delegates to get replacement values. It is inherently deferred. It allows great
    ///         degree of flexibility for caller, from where and when actual value will be obtained.
    ///     </para>
    ///     <para>
    ///         It is guaranteed, that duplicate keys across all dictionaries will not cause an exception. Bear in mind,
    ///         that in case of duplicates "last win".
    ///     </para>
    /// </remarks>
    public static string ReplacePlaceholders(
        string template,
        bool ignoreCase = true,
        params IReadOnlyDictionary<string, Func<string?>?>[] replacements)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var trie = new Trie(ignoreCase);

        foreach (var (key, valueProvider) in replacements.SelectMany(d => d))
        {
            trie.Insert(key, () => CleanString(valueProvider?.Invoke()));
        }

        return ReplacePlaceholdersWithTrie(template, trie);
    }

    /// <summary>
    ///     Cleans up replacement values from excess spaces and then replaces variables in template.
    /// </summary>
    /// <param name="template"></param>
    /// <param name="ignoreCase">
    ///     Defaults to ignore variable case sensitivity to be more unforgiving to user who manages
    ///     templating. On the flip-side it means that variable structure must be a bit more solid, like having more
    ///     complex boundary markers to distinguish variable from normal text with higher level of certainty.
    /// </param>
    /// <param name="replacements">Values are precalculated by caller.</param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         It is guaranteed, that duplicate keys across all dictionaries will not cause an exception. Bear in mind,
    ///         that in case of duplicates "last win".
    ///     </para>
    /// </remarks>
    public static string ReplacePlaceholders(
        string template,
        bool ignoreCase = true,
        params IReadOnlyDictionary<string, string?>[] replacements)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var trie = new Trie(ignoreCase);

        foreach (var (key, value) in replacements.SelectMany(d => d))
        {
            trie.Insert(key, () => CleanString(value));
        }

        return ReplacePlaceholdersWithTrie(template, trie);
    }

    private static string CleanString(string? input) =>
        string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : string.Join(
                ' ',
                input.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

    private static string ReplacePlaceholdersWithTrie(string template, Trie trie)
    {
        var result = new StringBuilder(template.Length);
        var currentIndex = 0;

        while (currentIndex < template.Length)
        {
            if (trie.Search(template, currentIndex) is { IsMatch: true } match)
            {
                // Add the cached replacement value to the result
                result.Append(match.ComputedValue);
                // Move index forward by matched length
                currentIndex += match.KeyLength;
            }
            else
            {
                // No match found, append the current character and move to the next
                result.Append(template[currentIndex]);
                currentIndex++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     This implementation in <see cref="TemplateProcessor" /> specifid, hence private.
    /// </summary>
    /// <remarks>
    ///     a) will match complete matches only;
    ///     b) has configurable case sensitivity for search phrase.
    /// </remarks>
    private class Trie
    {
        private readonly TrieNode _root = new();
        private readonly bool _ignoreCase;

        public Trie(bool ignoreCase) => _ignoreCase = ignoreCase;

        /// <summary>
        ///     Insert a key-value pair into the Trie.
        /// </summary>
        /// <param name="key">"Variable", placeholder.</param>
        /// <param name="valueProvider">
        ///     Calling this delegate should provide a value for placeholder. If providers in <c>null</c> then value
        ///     will become to <see cref="string.Empty" />
        /// </param>
        public void Insert(string key, Func<string?>? valueProvider)
        {
            // Using ToUpperInvariant() for consistent case-insensitive matching.
            var normalizedKey = _ignoreCase
                ? key.Normalize(NormalizationForm.FormC).ToUpperInvariant()
                : key.Normalize(NormalizationForm.FormC);

            var current = normalizedKey.Aggregate(_root, (acc, ch) => acc.GetOrAddChild(NormalizeCharacter(ch)));

            current.ValueProvider = valueProvider ?? (() => string.Empty);
        }

        /// <summary>
        ///     Search for the longest match in the Trie starting from a given index in the template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal SearchResult Search(string template, int startIndex)
        {
            if (string.IsNullOrEmpty(template) || startIndex >= template.Length)
            {
                return new(false, ReadOnlyMemory<char>.Empty, 0);
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index cannot be negative.");
            }

            TrieNode? current = null;
            var matchedLength = 0;

            for (var i = startIndex; i < template.Length; i++)
            {
                var ch = NormalizeCharacter(template[i]);

                if (!(current ?? _root).Children.TryGetValue(ch, out var child))
                {
                    break;
                }

                current = child;
                matchedLength++;
            }

            // Detect if node was not found or node was not Terminal, e.g. match was partial.
            if (current is not { Children.Count: 0 } node)
            {
                return new(false, ReadOnlyMemory<char>.Empty, 0);
            }

            // Cache the computed value in the node as ReadOnlyMemory<char>; defaults to empty, if value wasn't yielded
            node.ValueCache ??= node.ValueProvider?.Invoke()?.AsMemory()
                                ?? ReadOnlyMemory<char>.Empty;

            // Use an explicit flag to indicate whether we have a match
            return new(true, node.ValueCache.Value, matchedLength);
        }

        private char NormalizeCharacter(char ch) => _ignoreCase ? char.ToUpperInvariant(ch) : ch;

        public record TrieNode
        {
            /// <summary>
            ///     Children dictionary is private, and access is controlled internally.
            /// </summary>
            private readonly Dictionary<char, TrieNode> _children = new();

            /// <summary>
            ///     Expose Children as a read-only collection.
            /// </summary>
            public IReadOnlyDictionary<char, TrieNode> Children => _children;

            public Func<string?>? ValueProvider { get; set; }

            /// <summary>
            ///     Already obtained value, it should be preferred over repeated value provider invocations that have a
            ///     potential to be costly.
            /// </summary>
            public ReadOnlyMemory<char>? ValueCache { get; set; }

            public TrieNode GetOrAddChild(char ch) =>
                _children.TryGetValue(ch, out var child)
                    ? child
                    : _children[ch] = new();
        }

        internal readonly record struct SearchResult(bool IsMatch, ReadOnlyMemory<char> ComputedValue, int KeyLength);
    }
}
