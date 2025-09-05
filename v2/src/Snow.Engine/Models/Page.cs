using System;
using System.Collections.Generic;
using System.Linq;
using Snow.Abstractions;
using Snow.Engine.Configuration;

namespace Snow.Engine.Models;

/// <summary>
/// Represents a static page.
/// </summary>
public class Page : IPage
{
    private string? _excerpt;

    public Page()
    {
        Title = string.Empty;
        Content = string.Empty;
        Url = string.Empty;
        Author = string.Empty;
        Email = string.Empty;
        Layout = string.Empty;
        FileName = string.Empty;
        Keywords = string.Empty;
        MetaDescription = string.Empty;
        MarkdownHeader = string.Empty;
        Categories = Array.Empty<string>();
        Settings = new Dictionary<string, object>();
    }

    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public string Content { get; set; }

    /// <inheritdoc />
    public string ContentExcerpt
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_excerpt))
                return _excerpt;

            return Content.Split(new[] { "<!--excerpt-->" }, StringSplitOptions.None)[0];
        }
        set => _excerpt = value;
    }

    /// <inheritdoc />
    public string Url { get; set; }

    /// <inheritdoc />
    public string Author { get; set; }

    /// <inheritdoc />
    public string Email { get; set; }

    /// <inheritdoc />
    public DateTime Date { get; set; }

    /// <inheritdoc />
    public string Layout { get; set; }

    /// <inheritdoc />
    public string FileName { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<string> Categories { get; set; }

    /// <inheritdoc />
    public string Keywords { get; set; }

    /// <inheritdoc />
    public string MetaDescription { get; set; }

    /// <inheritdoc />
    public PublishedStatus Published { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Settings { get; set; }

    /// <inheritdoc />
    public string MarkdownHeader { get; set; }

    /// <summary>
    /// Sets default values from site settings.
    /// </summary>
    /// <param name="settings">The site settings.</param>
    public virtual void SetDefaults(SiteSettings settings)
    {
        if (string.IsNullOrEmpty(Author))
            Author = settings.Author;
        
        if (string.IsNullOrEmpty(Email))
            Email = settings.Email;
        
        if (string.IsNullOrEmpty(Layout))
            Layout = settings.DefaultPageLayout;
    }

    /// <summary>
    /// Sets properties from parsed front matter settings.
    /// </summary>
    /// <param name="settings">The front matter settings.</param>
    public void SetFromFrontMatter(IDictionary<string, object> settings)
    {
        foreach (var setting in settings)
        {
            switch (setting.Key.ToLowerInvariant())
            {
                case "categories":
                case "category":
                    {
                        var categoriesText = setting.Value.ToString() ?? string.Empty;
                        var categories = categoriesText
                            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .OrderBy(x => x)
                            .ToArray();
                        
                        Categories = categories;
                        break;
                    }
                case "title":
                    Title = setting.Value.ToString() ?? string.Empty;
                    break;
                case "layout":
                    Layout = setting.Value.ToString() ?? string.Empty;
                    break;
                case "author":
                    Author = setting.Value.ToString() ?? string.Empty;
                    break;
                case "email":
                    Email = setting.Value.ToString() ?? string.Empty;
                    break;
                case "published":
                    {
                        if (Enum.TryParse<PublishedStatus>(setting.Value.ToString(), true, out var published))
                        {
                            Published = published;
                        }
                        else if (setting.Value.ToString()?.ToLowerInvariant() == "true")
                        {
                            Published = PublishedStatus.Published;
                        }
                        break;
                    }
                case "metadescription":
                    MetaDescription = setting.Value.ToString() ?? string.Empty;
                    break;
                case "tags":
                case "keywords":
                    Keywords = setting.Value.ToString() ?? string.Empty;
                    break;
                case "date":
                    if (DateTime.TryParse(setting.Value.ToString(), out var date))
                        Date = date;
                    break;
            }
        }
    }
}
