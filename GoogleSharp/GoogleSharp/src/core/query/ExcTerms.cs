// Standard Library
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Internal
using GoogleSharp.Src.Core.Engine;
using GoogleSharp.Src.Core.Structures;

namespace GoogleSharp.Src.Core.Query
{
    public class ExcTerms : Terms
    {
        private static readonly Regex Pattern = new Regex(@"\-(\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ExcTerms(string expression) : base()
        {
            Collect(expression, Pattern, 1);
        }

        public override HashSet<Document> GetResults(IInvertedIndex index)
        {
            var results = new HashSet<Document>();
            Tokens.ForEach(t => results.UnionWith(index.GetDocumentsOfToken(t)));
            return results;
        }
    }
}
