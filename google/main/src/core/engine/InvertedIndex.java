package main.src.core.engine;

import main.src.core.structures.Document;
import main.src.core.structures.Token;
import main.src.utils.FileHandler;

import java.util.*;


public class InvertedIndex implements IndexInterface {

    private HashMap<Token, HashSet<Document>> index;

    public InvertedIndex(ArrayList<Document> documents) {
        this.index = new HashMap<>();
        this.indexDocuments(documents);
    }

    public void indexDocument(Document doc){
        for (Token token : FileHandler.getDocumentTokens(doc)){
            if (!this.index.containsKey(token))
                this.index.put(token, new HashSet<Document>());
            this.index.get(token).add(doc);
        }
    }

    public void indexDocuments(ArrayList<Document> documents){
        for (Document doc: documents)
            indexDocument(doc);
    }

    public HashSet<Document> getDocumentsOfToken(Token token){
        HashSet<Document> result = this.index.get(token);
        return result == null ? new HashSet<Document>() : result;
    }
    
    public HashMap<Token, HashSet<Document>> getIndex(){
        return this.index;
    }
}