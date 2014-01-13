namespace Snow.StaticFileProcessors
{
    using Enums;
    using Extensions;
    using Models;
    using Nancy.Testing;
    using System;
    using System.IO;
    using System.Linq;

    public class PostsProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "posts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {

            var filteredPosts = snowyData.Files.Where(ShouldProcess).ToList();

            var pageSize = settings.PageSize;
            var skip = 0;
            var iteration = 1;
            var currentIteration = filteredPosts.Skip(skip).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)filteredPosts.Count / pageSize);

            TestModule.TotalPages = totalPages;

            while (currentIteration.Any())
            {
                var folder = skip <= 1 ? "" : "page" + iteration;

                TestModule.PostsPaged = currentIteration.ToList();
                TestModule.PageNumber = iteration;
                TestModule.HasNextPage = iteration < totalPages;
                TestModule.HasPreviousPage = iteration > 1 && totalPages > 1;
                TestModule.GeneratedUrl = (settings.SiteUrl + "/" + folder).TrimEnd('/') + "/";

                var result = snowyData.Browser.Post("/static");

                result.ThrowIfNotSuccessful(snowyData.File.File);

                var outputFolder = Path.Combine(snowyData.Settings.Output, folder);

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                File.WriteAllText(Path.Combine(outputFolder, "index.html"), result.Body.AsString());

                skip += pageSize;
                iteration++;
                currentIteration = filteredPosts.Skip(skip).Take(pageSize).ToList();
            }
        }

        private bool ShouldProcess(Post post)
        {
            return post.Published == Published.True;
        }
    }
}