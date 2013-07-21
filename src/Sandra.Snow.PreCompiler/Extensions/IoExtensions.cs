namespace Sandra.Snow.PreCompiler.Extensions
{
    using System.IO;

    public static class IoExtensions
    {
        public static void Empty(this DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles()) 
                file.Delete();

            foreach (var subDirectory in directory.GetDirectories()) 
                subDirectory.DeleteDirectory();
        }

        public static void DeleteDirectory(this DirectoryInfo directory)
        {
            var files = directory.GetFiles();
            var dirs = directory.GetDirectories();

            foreach (var file in files)
                file.Delete();

            foreach (var dir in dirs)
                DeleteDirectory(dir);

            directory.Delete(true);
        }

        public static void Copy(this DirectoryInfo sourceDirectory, string destDirName, bool copySubDirs)
        {
            var dirs = sourceDirectory.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectory);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            var files = sourceDirectory.GetFiles();

            foreach (var file in files)
            {
                // Create the path to the new copy of the file.
                var temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    subdir.Copy(temppath, true);
                }
            }
        }
    }
}