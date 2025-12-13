// Nancy compatibility shim - allows existing v1 templates with @inherits Nancy.ViewEngines.Razor.NancyRazorViewBase<T> to work
// ReSharper disable CheckNamespace
namespace Nancy.ViewEngines.Razor;

using Snow.Engine.Templates.Razor;

/// <summary>
/// Nancy-compatible base class for Razor templates.
/// This shim allows existing v1 templates that use @inherits Nancy.ViewEngines.Razor.NancyRazorViewBase&lt;T&gt; to work unchanged.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public abstract class NancyRazorViewBase<TModel> : SnowTemplateBase<TModel>
{
    // All functionality is inherited from SnowTemplateBase<TModel>
    // which provides the Html helper and layout support
}

/// <summary>
/// Non-generic base for dynamic models.
/// </summary>
public abstract class NancyRazorViewBase : NancyRazorViewBase<dynamic>
{
}
