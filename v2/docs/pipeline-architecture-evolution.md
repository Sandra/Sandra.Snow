# Pipeline Architecture Evolution Plan

## Current State (Stage 4)

### SiteGenerator Approach
The current implementation uses a **straightforward SiteGenerator** that handles the complete workflow internally:

```csharp
public class SiteGenerator 
{
    public async Task GenerateAsync(CancellationToken cancellationToken)
    {
        // Step 1: Parse all content
        var (posts, pages) = await ParseAllContentAsync(cancellationToken);
        
        // Step 2: Render posts and pages  
        await RenderPostsAndPagesAsync(posts, pages, cancellationToken);
        
        // Step 3: Process static files
        await ProcessStaticFilesAsync(posts, pages, cancellationToken);
        
        // Step 4: Copy assets
        await CopyAssetsAsync(cancellationToken);
    }
}
```

### Benefits of Current Approach
- ✅ **Simple & Working**: Direct, easy-to-understand workflow
- ✅ **Testable**: Clear integration points for testing
- ✅ **Fast Development**: Rapid implementation and debugging
- ✅ **Complete**: Handles entire site generation end-to-end

### Limitations 
- **Memory Usage**: All content loaded into memory at once
- **Limited Extensibility**: Hard to add middleware/transformations
- **Sequential Processing**: No streaming or parallel processing benefits

## Future Evolution: PagePipe Architecture

### Target Architecture (Stage 6+)
Migrate to the Oracle-recommended streaming pipeline approach:

```csharp
public delegate IAsyncEnumerable<PipelinePage> PagePipe(
    IAsyncEnumerable<PipelinePage> source, 
    CancellationToken cancellationToken = default);

// Pipeline composition
var pipeline = pipes.Aggregate(
    seed => seed,          // identity
    (next, pipe) => src => pipe(next(src), token));
```

### Migration Strategy

#### Phase 1: Extract Internal Steps (Stage 6)
Extract current SiteGenerator steps into reusable components:

```csharp
public class ParsePipe : IAsyncEnumerable<PipelinePage> { ... }
public class RenderPipe : IAsyncEnumerable<PipelinePage> { ... }  
public class WritePipe : IAsyncEnumerable<PipelinePage> { ... }
```

#### Phase 2: Streaming Implementation (Stage 7)
Replace in-memory collection with streaming:

```csharp
await foreach (var page in pipeline(reader.ReadAllAsync(token)))
{
    // Page is processed and written incrementally
}
```

#### Phase 3: Middleware Support (Stage 8+)
Enable user-defined transformations:

```csharp
services.AddPagePipe<ImageOptimizationPipe>();
services.AddPagePipe<SyntaxHighlightingPipe>();
services.AddPagePipe<InternationalizationPipe>();
```

## Benefits of Evolution

### Memory Efficiency
- **Current**: O(N) memory for all content
- **Future**: O(1) memory per page processed

### Performance
- **Current**: Sequential processing
- **Future**: Parallel processing where safe

### Extensibility  
- **Current**: Modify SiteGenerator for new features
- **Future**: Add middleware pipes without core changes

### Composability
- **Current**: Fixed workflow
- **Future**: User-configurable pipeline composition

## Implementation Timeline

- **Stage 4**: ✅ Working SiteGenerator foundation
- **Stage 5**: CLI interface using SiteGenerator
- **Stage 6**: Extract pipes while maintaining SiteGenerator API
- **Stage 7**: Implement streaming pipeline
- **Stage 8**: Add middleware and extensibility

## Compatibility Promise

The **ISiteGenerator interface** will remain stable throughout evolution:

```csharp
public interface ISiteGenerator
{
    Task GenerateAsync(CancellationToken cancellationToken = default);
}
```

This ensures CLI and other consumers continue working as the internal architecture evolves.

## Conclusion

The current SiteGenerator provides a **solid foundation** that delivers working functionality immediately while providing a clear **migration path** to advanced pipeline architecture. This balances immediate delivery needs with long-term scalability and extensibility goals.
