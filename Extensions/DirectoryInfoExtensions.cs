using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Seyyedi
{
	public static class DirectoryInfoExtensions
	{
		public static FileInfo GetFileInfo(this DirectoryInfo directory, params string[] paths)
			=> new FileInfo(
				Path.Combine(
                    new [] { directory.FullName }
						.Concat(paths)
						.ToArray()
				)
			);

		public static DirectoryInfo GetDirectoryInfo(this DirectoryInfo directory, params string[] paths)
			=> new DirectoryInfo(
				Path.Combine(
					new[] { directory.FullName }
						.Concat(paths)
						.ToArray()
				)
			);

		public static IEnumerable<FileInfo> EnumerateFilesRecursive(this DirectoryInfo directory)
			=> directory
				.EnumerateFiles()
				.Concat(directory
					.EnumerateDirectories()
					.SelectMany(EnumerateFilesRecursive)
				);

		public static DirectoryInfo Ensure(this DirectoryInfo directory)
		{
			if (!Directory.Exists(directory.FullName))
			{
				directory.Create();
			}

			return directory;
		}
	}
}