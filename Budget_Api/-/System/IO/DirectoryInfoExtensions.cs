using System.Collections.Generic;
using System.Linq;

namespace System.IO
{
	internal static class DirectoryInfoExtensions
	{
		public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directory,
		                                                   Func<FileInfo, bool> predicate,
		                                                   SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			foreach (FileInfo fileInfo in directory.GetFiles().Where(predicate))
			{
				yield return fileInfo;
			}
			if (searchOption == SearchOption.AllDirectories)
			{
				foreach (FileInfo fileInfo in directory.GetDirectories().SelectMany(child => child.EnumerateFiles(predicate, searchOption)))
				{
					yield return fileInfo;
				}
			}
		}

		public static FileInfo FindFirst(this DirectoryInfo directoryInfo,
		                                 string pattern,
		                                 SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return directoryInfo.EnumerateFiles(pattern, searchOption).FirstOrDefault();
		}

		public static FileInfo FindFirst(this DirectoryInfo directoryInfo,
		                                 Func<FileInfo, bool> predicate,
		                                 SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return directoryInfo.EnumerateFiles(predicate, searchOption).FirstOrDefault();
		}
	}
}