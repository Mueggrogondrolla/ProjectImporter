using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Runtime;
using Path = System.IO.Path;

namespace AST_ProjectImporter
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();

			AssignmentDirectory = Properties.Settings.Default.AssignmentDirectory;
			ProjectPrefix = Properties.Settings.Default.ProjectPrefix;
			MasterSolutionPath = Properties.Settings.Default.MasterSolutionPath;
			AdditionalDependencies = Properties.Settings.Default.AdditionalDependencies;
			AdditionalIncludes = Properties.Settings.Default.AdditionalIncludes;

			random = new Random(DateTime.Now.Millisecond);
			Errors = new ObservableCollection<string>();

			DataContext = this;
		}

		private string projectPrefix;
		public string ProjectPrefix
		{
			get => projectPrefix;
			set
			{
				projectPrefix = value;
				NotifyPropertyChanged("ProjectPrefix");

				Properties.Settings.Default.ProjectPrefix = ProjectPrefix;
				Properties.Settings.Default.Save();
			}
		}

		private string assignmentDirectory;
		public string AssignmentDirectory
		{
			get => assignmentDirectory;
			set
			{
				assignmentDirectory = value;
				NotifyPropertyChanged("AssignmentDirectory");

				Properties.Settings.Default.AssignmentDirectory = AssignmentDirectory;
				Properties.Settings.Default.Save();
			}
		}

		private string additionalDependencies;
		public string AdditionalDependencies
		{
			get => additionalDependencies;
			set
			{
				additionalDependencies = value;
				NotifyPropertyChanged("AdditionalDependencies");

				Properties.Settings.Default.AdditionalDependencies = AdditionalDependencies;
				Properties.Settings.Default.Save();
			}
		}

		private string additionalIncludes;
		public string AdditionalIncludes
		{
			get => additionalIncludes;
			set
			{
				additionalIncludes = value;
				NotifyPropertyChanged("AdditionalIncludes");

				Properties.Settings.Default.AdditionalIncludes = AdditionalIncludes;
				Properties.Settings.Default.Save();
			}
		}

		private string masterSolutionPath;
		public string MasterSolutionPath
		{
			get => masterSolutionPath;
			set
			{
				masterSolutionPath = value;
				NotifyPropertyChanged("MasterSolutionPath");

				Properties.Settings.Default.MasterSolutionPath = MasterSolutionPath;
				Properties.Settings.Default.Save();
			}
		}

		public double Progress { get; set; }

		public bool AutoImportSourceFiles { get; set; } = true;

		public bool AutoImportToMaster { get; set; } = true;

		public ObservableCollection<string> Errors { get; }

		private readonly Random random;


		public event PropertyChangedEventHandler PropertyChanged;

		private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!Directory.Exists(AssignmentDirectory))
				{
					Errors.Add("Assignment directory does not exist!");
					return;
				}
				string[] subDirectories = Directory.GetDirectories(AssignmentDirectory, "*", SearchOption.TopDirectoryOnly);

				for (int i = 0; i < subDirectories.Length; i++)
				{
					// clean up the folder names (remove everything behind the first underscore)
					string cleanedDirectoryName = Path.GetFileName(subDirectories[i]).Split('_')[0];
					string cleanedDirectoryPath = Path.Combine(AssignmentDirectory, cleanedDirectoryName);

					if (Path.Combine(AssignmentDirectory, subDirectories[i]) == cleanedDirectoryPath) { continue; }

					Directory.Move(Path.Combine(AssignmentDirectory, subDirectories[i]), cleanedDirectoryPath);

					// Get the name of the submitter
					string projectName = ProjectPrefix + cleanedDirectoryName.Replace(" ", "");

					// Generate the project folder
					string projectDirectoryPath = Path.Combine(cleanedDirectoryPath, projectName);
					Directory.CreateDirectory(projectDirectoryPath);


					string[] headerFiles = new string[0];
					string[] cppFiles = new string[0];

					if (AutoImportSourceFiles)
					{
						ExtractAndSearchSourceFiles(cleanedDirectoryPath, out headerFiles, out cppFiles);
					}

					string guid = GenerateGuid();
					GenerateProjectFile(projectDirectoryPath, projectName, guid, headerFiles, cppFiles);

					if (AutoImportToMaster)
					{
						WriteIntoSlnFile(projectDirectoryPath, projectName, guid);
					}

					Progress = (1.0 + i) / subDirectories.Length;
					NotifyPropertyChanged();
				}
			}
			catch(Exception ex)
			{
				System.Windows.MessageBox.Show("Something went wrong!\r\n\r\n" + ex.ToString());
			}
		}

		private void ButtonBrowseAssignmentDirectory_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true,
				SelectedPath = AssignmentDirectory
			};

			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				AssignmentDirectory = folderBrowserDialog.SelectedPath;
			}
		}

		private void ButtonBrowseMasterSolutionPath_Click(object sender, RoutedEventArgs e)
		{
			FileDialog fileDialog = new OpenFileDialog()
			{
				Multiselect = false,
				FileName = Properties.Settings.Default.MasterSolutionPath
			};

			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				MasterSolutionPath = fileDialog.FileName;
			}
		}

		private void NotifyPropertyChanged(string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private static void ExtractAndSearchSourceFiles(string cleanedDirectoryPath, out string[] headerFiles, out string[] cppFiles)
		{
			string[] submittedFiles = Directory.GetFiles(cleanedDirectoryPath);

			for (int j = 0; j < submittedFiles.Length; j++)
			{
				if (Path.GetExtension(submittedFiles[j]) == ".zip" && !File.Exists(Path.GetFileNameWithoutExtension(submittedFiles[j])))
				{
					// can fail if e.g. the directory path or a path of an extracted file is too long
					try
					{
						System.IO.Compression.ZipFile.ExtractToDirectory(submittedFiles[j], cleanedDirectoryPath + "\\" + Path.GetFileNameWithoutExtension(submittedFiles[j]));
					}
					catch (Exception) { }
				}
			}

			string[] headerFilesRaw = Directory.GetFiles(cleanedDirectoryPath, "*.h", SearchOption.AllDirectories);
			string[] cppFilesRaw = Directory.GetFiles(cleanedDirectoryPath, "*.cpp", SearchOption.AllDirectories);

			headerFiles = new string[headerFilesRaw.Length];
			cppFiles = new string[cppFilesRaw.Length];

			for (int j = 0; j < headerFilesRaw.Length; j++) { headerFiles[j] = headerFilesRaw[j].Replace(cleanedDirectoryPath, "..").Replace("\\", "/"); }
			for (int j = 0; j < cppFilesRaw.Length; j++) { cppFiles[j] = cppFilesRaw[j].Replace(cleanedDirectoryPath, "..").Replace("\\", "/"); }
		}

		private void GenerateProjectFile(string path, string projectName, string guid, string[] headerFiles, string[] cppFiles)
		{
			string template = Properties.Resources.ProjectFileTemplate;
			template = template.Replace("${ProjectGuid}", guid);
			template = template.Replace("${ProjectName}", projectName);
			template = template.Replace("${AdditionalIncludeDirectories}", GetAdditionalIncludeDirectories());
			template = template.Replace("${AdditionalDependencies}", GetAdditionalDependencies());
			template = template.Replace("${HeaderFiles}", GetHeaderFilesIncludeString(headerFiles));
			template = template.Replace("${CppFiles}", GetCppFilesIncludeString(cppFiles));
			File.WriteAllText(path + "\\" + projectName + ".vcxproj", template);

			string templateFilters = Properties.Resources.filtersTemplate;
			templateFilters = templateFilters.Replace("${HeaderFiles}", GetHeaderFilesIncludeString(headerFiles, true));
			templateFilters = templateFilters.Replace("${CppFiles}", GetCppFilesIncludeString(cppFiles, true));
			File.WriteAllText(path + "\\" + projectName + ".vcxproj.filters", templateFilters);

			File.WriteAllText(path + "\\" + projectName + ".vcxproj.user", Properties.Resources.userTemplate);
		}

		private void WriteIntoSlnFile(string path, string projectName, string guid)
		{
			if (!File.Exists(MasterSolutionPath))
			{
				Errors.Add("Could not import projects into master project. Master solution does not exist!");
				return;
			}

			string relativePath = GetRelativePath(MasterSolutionPath, path);

			string projectString = "Project(\"{" + guid.ToUpper() + "}\") = \"" + projectName + "\", \"" + relativePath + "/" + projectName + ".vcxproj" + "\", \"{" + guid.ToUpper() + "}\"\r\nEndProject";

			StringBuilder stringBuilder = new StringBuilder();
			bool stringAdded = false;

			try
			{
				using (StreamReader reader = new StreamReader(MasterSolutionPath))
				{
					while (!reader.EndOfStream)
					{
						string currentLine = reader.ReadLine();

						if (currentLine.StartsWith("Project") && !stringAdded)
						{
							stringBuilder.AppendLine(projectString);
							stringAdded = true;
						}
						stringBuilder.AppendLine(currentLine);
					}
				}
				File.WriteAllText(MasterSolutionPath, stringBuilder.ToString());
			}
			catch (Exception)
			{
				Errors.Add("Could not import projects into master project. Can not read or write master solution file!");
				return;
			}
		}

		private string GetHeaderFilesIncludeString(string[] headerFiles, bool exportFilterInformation = false)
		{
			string stringToReturn = "";

			for (int i = 0; i < headerFiles.Length; i++)
			{
				stringToReturn += "<ClInclude Include=\"" + headerFiles[i] + "\">";
				if (exportFilterInformation) { stringToReturn += "<Filter>Header Files</Filter>"; }
				stringToReturn += "</ClInclude>";
			}

			return stringToReturn;
		}

		private string GetCppFilesIncludeString(string[] cppFiles, bool exportFilterInformation = false)
		{
			string stringToReturn = "";

			for (int i = 0; i < cppFiles.Length; i++)
			{
				stringToReturn += "<ClCompile Include=\"" + cppFiles[i] + "\">";
				if (exportFilterInformation) { stringToReturn += "<Filter>Source Files</Filter>"; }
				stringToReturn += "</ClCompile>";
			}

			return stringToReturn;
		}

		private string GenerateGuid()
		{
			string guid = "";
			string possibleChars = "0123456789abcdef";

			for (int i = 0; i < 8; i++) { guid += possibleChars[random.Next(possibleChars.Length)]; }
			guid += "-";
			for (int i = 0; i < 4; i++) { guid += possibleChars[random.Next(possibleChars.Length)]; }
			guid += "-";
			for (int i = 0; i < 4; i++) { guid += possibleChars[random.Next(possibleChars.Length)]; }
			guid += "-";
			for (int i = 0; i < 4; i++) { guid += possibleChars[random.Next(possibleChars.Length)]; }
			guid += "-";
			for (int i = 0; i < 12; i++) { guid += possibleChars[random.Next(possibleChars.Length)]; }

			return guid;
		}

		private string GetAdditionalIncludeDirectories() => "<AdditionalIncludeDirectories>" + AdditionalIncludes.Replace("\\\\", "\\") + "</AdditionalIncludeDirectories>";

		private string GetAdditionalDependencies() => "<AdditionalDependencies>" + AdditionalDependencies.Replace("\\\\", "\\") + "</AdditionalDependencies>";

		/// <summary>
		/// Source code from here: https://stackoverflow.com/questions/51179331/is-it-possible-to-use-path-getrelativepath-net-core2-in-winforms-proj-targeti
		/// </summary>
		/// <param name="relativeTo"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public string GetRelativePath(string relativeTo, string path)
		{
			var uri = new Uri(relativeTo);
			var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
			{
				rel = $".{ Path.DirectorySeparatorChar }{ rel }";
			}
			return rel;
		}
	}
}
