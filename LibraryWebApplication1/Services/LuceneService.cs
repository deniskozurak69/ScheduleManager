using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Search;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Apis.Drive.v3.Data;
public class LuceneService
{
    private readonly Lucene.Net.Store.Directory _indexDirectory;
    private readonly Lucene.Net.Analysis.Analyzer _analyzer;
    private readonly IndexWriter _writer;
    public LuceneService()
    {
        _indexDirectory = FSDirectory.Open(new DirectoryInfo(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "index")));
        _analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30); 
        _writer = new IndexWriter(_indexDirectory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
    }
    public void ClearIndex()
    {
        var matchAllQuery = new MatchAllDocsQuery();
        _writer.DeleteDocuments(matchAllQuery);
        _writer.Commit();
    }
    public void IndexUser(LibraryWebApplication1.Models.User user)
    {
        _writer.DeleteDocuments(new Term("UserId", user.UserId.ToString()));
        var doc = new Lucene.Net.Documents.Document();
        doc.Add(new Lucene.Net.Documents.Field("UserId", user.UserId.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("Username", user.Username, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("ProfilePhoto", user.ProfilePhoto, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
        _writer.AddDocument(doc);
        _writer.Commit();
    }
    public void IndexCategory(Category category)
    {
        _writer.DeleteDocuments(new Term("CategoryId", category.CategoryId.ToString()));
        var doc = new Document();
        doc.Add(new Lucene.Net.Documents.Field("CategoryId", category.CategoryId.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("Name", category.Name, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("Description", category.Description, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        _writer.AddDocument(doc);
        _writer.Commit();
    }
    public void IndexArticle(Article article)
    {
        _writer.DeleteDocuments(new Term("ArticleId", article.ArticleId.ToString()));
        var doc = new Document();
        doc.Add(new Lucene.Net.Documents.Field("ArticleId", article.ArticleId.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("ArticleName", article.ArticleName, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("PublishDate", article.PublishDate.ToString(), Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        doc.Add(new Lucene.Net.Documents.Field("Text", article.Text, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
        _writer.AddDocument(doc);
        _writer.Commit();
    }
    public void AddAllToIndex(IEnumerable<Category> categories, IEnumerable<Article> articles, IEnumerable<LibraryWebApplication1.Models.User> users)
    {
        foreach (var category in categories)
        {
            IndexCategory(category);
        }
        foreach (var article in articles)
        {
            IndexArticle(article);
        }
        foreach (var user in users)
        {
            IndexUser(user);
        }
    }
    public async Task<IEnumerable<object>> SearchCategoryAsync(string queryText)
    {
        var searcher = new IndexSearcher(_indexDirectory, true);
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Name", _analyzer);
        var query = parser.Parse(queryText);
        var results = searcher.Search(query, 10);
        var categories = results.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).Select(doc => new
        {
            CategoryId = int.Parse(doc.Get("CategoryId")),
            Name = doc.Get("Name"),
            Description = doc.Get("Description")
        });
        return categories;
    }
    public async Task<IEnumerable<object>> SearchArticleAsync(string queryText)
    {
        var searcher = new IndexSearcher(_indexDirectory, true);
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "ArticleName", _analyzer);
        var query = parser.Parse(queryText);
        var results = searcher.Search(query, 10);
        var articles = results.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).Select(doc => new
        {
            ArticleId = int.Parse(doc.Get("ArticleId")),
            ArticleName = doc.Get("ArticleName"),
            PublishDate = doc.Get("PublishDate"),
            Text = doc.Get("Text")
        });
        return articles;
    }
    public async Task<IEnumerable<object>> SearchUserAsync(string queryText)
    {
        var searcher = new IndexSearcher(_indexDirectory, true);
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Username", _analyzer);
        var query = parser.Parse(queryText);
        var results = searcher.Search(query, 10);
        var users = results.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).Select(doc => new
        {
            UserId = int.Parse(doc.Get("UserId")),
            Username = doc.Get("Username"),
            Password = doc.Get("Password"),
            ProfilePhoto = doc.Get("ProfilePhoto")
        });
        return users;
    }
}