namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System;
    using System.IO;
    using System.Linq;
    using Nancy.Testing;

    public class PostsPagedProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "postspaged"; }
        }

        public override void Process(SnowyData snowyData)
        {
            const int pageSize = 10;
            var skip = 0;
            var iteration = 1;
            var currentIteration = snowyData.Files.Skip(skip).Take(pageSize).ToList();
            var totalPages = (int)Math.Ceiling((double)snowyData.Files.Count() / pageSize);

            TestModule.TotalPages = totalPages;

            while (currentIteration.Any())
            {
                TestModule.PostsPaged = currentIteration.Select(x => x.Post).ToList();
                TestModule.PageNumber = iteration;
                TestModule.HasNextPage = iteration < totalPages;
                TestModule.HasPreviousPage = iteration > 2 && totalPages > 1;

                var result = snowyData.Browser.Post("/static");

                result.StatusCode.ThrowIfNotSuccessful();

                var folder = skip == 0 ? "" : "page" + iteration;
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