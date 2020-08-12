using System.Collections.Generic;
using GoogleSharp.Src.Core.Structures;
using GoogleSharp.Src.Utils;

namespace GoogleSharp.Src.Core.Engine
{
    public class GoogleInvertedIndex : IInvertedIndex
    {
        public Dictionary<Token, HashSet<Document>> Index { get; set; }

        public GoogleInvertedIndex(List<Document> documents)
        {
            this.Index = new Dictionary<Token, HashSet<Document>>();
            this.IndexDocuments(documents);
        }

        private void IndexDocument(Document doc)
        {
            foreach (var token in FileHandler.GetFileTokens(doc.Path))
            {
                if (!this.Index.ContainsKey(token))
                    this.Index.Add(token, new HashSet<Document>());
                this.Index[token].Add(doc);
            }
        }

        private void IndexDocuments(List<Document> documents)
        {
            foreach (var doc in documents)
                IndexDocument(doc);
        }

        public HashSet<Document> GetDocumentsOfToken(Token token)
        {
            var result = this.Index[token];
            return result == null ? new HashSet<Document>() : result;
        }
    }
}