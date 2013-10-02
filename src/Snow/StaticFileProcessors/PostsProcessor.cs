namespace Snow.StaticFileProcessors
{
    using System;
    using System.IO;
    using System.Linq;
    using Extensions;
    using Nancy.Testing;

    public class PostsProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "posts"; }
        }

        protected override void Impl(SnowyData snowyData, SnowSettings settings)
        {
            const int pageSize = 10;
            var skip = 0;
            var iteration = 1;
            var currentIteration = snowyData.Files.Skip(skip).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)snowyData.Files.Count() / pageSize);

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
                currentIteration = snowyData.Files.Skip(skip).Take(pageSize).ToList();
            }
        }
    }
}