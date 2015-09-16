using System.IO;

namespace Seyyedi
{
	public static class FileInfoExtensions
	{
		public static void CopyTo(this FileInfo file, FileInfo target, bool overwrite = true)
			=> file.CopyTo(target.FullName, overwrite);

		public static void MoveTo(this FileInfo file, FileInfo target)
			=> file.MoveTo(target.FullName);
	}
}