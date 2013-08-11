namespace Sandra.Snow.PreCompiler.StaticFileProcessors
{
    using System;
    using System.IO;
    using System.Linq;
    using Nancy.Testing;
    using Sandra.Snow.PreCompiler.Extensions;

    public class PostsProcessor : BaseProcessor
    {
        public override string ProcessorName
        {
            get { return "posts"; }
        }

        public override bool IterateModel
        {
            get { return true; }
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
                TestModule.PostsPaged = currentIteration.ToList();
                TestModule.PageNumber = iteration;
                TestModule.HasNextPage = iteration < totalPages;
                TestModule.HasPreviousPage = iteration > 1 && totalPages > 1;

                var result = snowyData.Browser.Post("/static");

                result.StatusCode.ThrowIfNotSuccessful();

                var folder = skip <= 1 ? "" : "page" + iteration;
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