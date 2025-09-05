using System.Collections.Generic;
using System.Linq;
using Snow.Abstractions;

namespace Snow.Engine.Models;

/// <summary>
/// Represents a category with its associated posts.
/// </summary>
public class Category : ICategory
{
    private readonly List<IPost> _posts;

    public Category(string name)
    {
        Name = name;
        Slug = name.ToLowerInvariant().Replace(' ', '-');
        _posts = new List<IPost>();
    }

    public Category(string name, string slug) : this(name)
    {
        Slug = slug;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Slug { get; }

    /// <inheritdoc />
    public IReadOnlyList<IPost> Posts => _posts.AsReadOnly();

    /// <inheritdoc />
    public int PostCount => _posts.Count;

    /// <summary>
    /// Adds a post to this category.
    /// </summary>
    /// <param name="post">The post to add.</param>
    public void AddPost(IPost post)
    {
        if (!_posts.Contains(post))
        {
            _posts.Add(post);
        }
    }

    /// <summary>
    /// Adds multiple posts to this category.
    /// </summary>
    /// <param name="posts">The posts to add.</param>
    public void AddPosts(IEnumerable<IPost> posts)
    {
        foreach (var post in posts.Where(p => !_posts.Contains(p)))
        {
            _posts.Add(post);
        }
    }

    /// <summary>
    /// Sorts posts in the category by date descending.
    /// </summary>
    public void SortPosts()
    {
        _posts.Sort((x, y) => y.Date.CompareTo(x.Date));
    }
}
