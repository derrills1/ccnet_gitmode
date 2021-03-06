using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol
{
	public class PvcsHistoryParser : IHistoryParser
	{
		private static Regex _searchRegEx = new Regex(@"(?<Archive>Archive:\s+.*?\r\n(.|\s)*?(={35}(\r\n|$)))", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
		private static Regex _archiveRegEx = new Regex(@"Archive:\s+(?<ArchiveName>.*?)\nWorkfile:\s+(?<Filename>.*?)\nArchive\screated:\s+(?<CreatedDate>.*?)\r\n(.|\s)*?-{35}\r\n(?<Revision>Rev\s\d+(\.\d+)*\r\n(.*\r\n)*?Author\sid:.*?\r\n((?!(={35}|-{35}))(.|\s)*?\r\n)?(-{35}|={35})(\r\n|$))+", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
		private static Regex _revisionRegEx = new Regex(@"Rev\s(?<Version>\d(\.\d+)*)\r\n(.*?\r\n)?Checked\sin:\s+(?<CheckIn>.*?)\r\n(.*?\r\n)?Last\smodified:\s+(?<PreviousModification>.*?)\r\n(.*?\r\n)?Author\sid:\s+(?<Author>.*?)\s.*?\r\n(Branches:\s+.*?\r\n)?(?<Comment>(((?!(={35}|-{35}))(.|\s)*?)\r\n)?)(={35}|-{35})(\r\n|$)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

		public Modification[] Parse(TextReader reader, DateTime from, DateTime to)
		{
			string modificationFile = reader.ReadToEnd();
            var mods = new List<Modification>();

			MatchCollection matches = _searchRegEx.Matches(modificationFile);
			foreach (Match archive in matches)
			{
				// We have an archive, now find modifications
				MatchCollection archives = _archiveRegEx.Matches(archive.Value);
				foreach (Match archiveDetails in archives)
				{
					ParseArchive(archiveDetails, mods);
				}
			}
			// Ensure that duplicates were not pulled back in the Vlog (this can be caused by doing a get using Labels)
			return AnalyzeModifications(mods);
		}

		private static void ParseArchive(Match archive, IList modifications)
		{
			string archivePath = archive.Groups["ArchiveName"].Value.Trim();
			DateTime createdDate = Pvcs.GetDate(archive.Groups["CreatedDate"].Value.Trim());

			MatchCollection revisions = _revisionRegEx.Matches(archive.Value);
			foreach (Match revision in revisions)
			{
				modifications.Add(ParseModification(revision, archivePath, createdDate));
			}
		}

		private static Modification ParseModification(Match revision, string path, DateTime createdDate)
		{
			Modification mod = new Modification();
			mod.Comment = revision.Groups["Comment"].Value.Trim();
			mod.FileName = Path.GetFileName(path);
			mod.FolderName = Path.GetDirectoryName(path).Trim();
			mod.ModifiedTime = Pvcs.GetDate(revision.Groups["CheckIn"].Value.Trim());
			mod.UserName = revision.Groups["Author"].Value.Trim();
			mod.Version = revision.Groups["Version"].Value.Trim();
			mod.Type = (mod.ModifiedTime == createdDate) ? "New" : "Checked in";
			return mod;
		}

		/// <summary>
		/// Build the Modification list of what files will be built 
		/// with this Release
		/// </summary>
		/// <param name="mods"></param>
		/// <returns></returns>
		public static Modification[] AnalyzeModifications(IList mods)
		{
			// Hashtables are used so we can compare on the keys in search of duplicates
			SortedList allFiles = new SortedList();
			foreach (Modification mod in mods)
			{
				string key = mod.FolderName + mod.FileName;
				if (!allFiles.ContainsKey(key))
					allFiles.Add(key, mod);
				else
				{
					// If the revision number on the original is larger, then
					// do the comparision against the original modification
					// in search to see which revision is higher
					// example: 1.64.1 < 1.65 but you need to compare against the 
					// larger string of 1.64.1 because we are splitting the numbers individually
					// so 1 is compared to 1 and 64 is compared to 65.
					Modification compareMod = allFiles[key] as Modification;
					string[] originalVersion = compareMod.Version.Split(char.Parse("."));
					string[] currentVersion = mod.Version.Split(char.Parse("."));
					int len1 = originalVersion.Length;
					int len2 = currentVersion.Length;
					int usingLen;
					int otherLen;
					if (len1 >= len2)
					{
						usingLen = len1;
						otherLen = len2;
					}
					else
					{
						usingLen = len2;
						otherLen = len1;
					}

					for (int i = 0; i < usingLen; i++)
					{
						if (i > otherLen)
							continue;
						if (Convert.ToInt32(currentVersion[i]) > Convert.ToInt32(originalVersion[i]))
						{
							allFiles[compareMod.FolderName + compareMod.FileName] = mod;
							break;
						}
					}
				}
			}
			// Convert the Hashtables to Modification arrays
			Modification[] validMods = new Modification[allFiles.Count];
			int count = 0;
			foreach (string key in allFiles.Keys)
			{
				validMods[count++] = allFiles[key] as Modification;
			}
			return validMods;
		}
	}
}