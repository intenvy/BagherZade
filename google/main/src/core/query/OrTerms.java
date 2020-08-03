package main.src.core.query;

import java.util.regex.Pattern;
import java.util.*;

import main.src.core.engine.InvertedIndex;
import main.src.core.structures.Token;
import main.src.core.structures.Document;


public class OrTerms extends Terms{

    private static Pattern pattern = Pattern.compile("\\+(\\w+)");
    private static int regexGroupIndex = 1;

    public OrTerms(String expression){
        this.keys = new ArrayList<>();
        this.collect(expression, pattern, regexGroupIndex);
    }

    public HashSet<Document> getResults(InvertedIndex index){
        HashSet<Document> results = new HashSet<>();
        for(Token token : this.keys)
            results.addAll(index.getDocumentsOfToken(token));
        return results;
    }
}
