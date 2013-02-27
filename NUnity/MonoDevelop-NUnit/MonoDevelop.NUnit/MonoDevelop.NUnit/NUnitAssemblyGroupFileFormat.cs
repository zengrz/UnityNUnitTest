using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Projects;
using MonoDevelop.Projects.Extensions;
using MonoDevelop.Projects.Formats.MD1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace MonoDevelop.NUnit
{
	public class NUnitAssemblyGroupFileFormat : IFileFormat
	{
		public string Name
		{
			get
			{
				return "NUnit assembly group";
			}
		}
		public bool SupportsMixedFormats
		{
			get
			{
				return false;
			}
		}
		public FilePath GetValidFormatName(object obj, FilePath fileName)
		{
			return fileName.ChangeExtension(".md-nunit");
		}
		public bool CanReadFile(FilePath file, System.Type expectedType)
		{
			return expectedType.IsAssignableFrom(typeof(NUnitAssemblyGroupProject)) && System.IO.Path.GetExtension(file) == ".md-nunit";
		}
		public bool CanWriteFile(object obj)
		{
			return false;
		}
		public void WriteFile(FilePath file, object obj, IProgressMonitor monitor)
		{
			this.WriteFile(file, file, obj, monitor);
		}
		public void ExportFile(FilePath file, object obj, IProgressMonitor monitor)
		{
			this.WriteFile(((NUnitAssemblyGroupProject)obj).get_FileName(), file, obj, monitor);
		}
		public System.Collections.Generic.List<FilePath> GetItemFiles(object obj)
		{
			return new System.Collections.Generic.List<FilePath>();
		}
		private void WriteFile(FilePath file, FilePath outFile, object obj, IProgressMonitor monitor)
		{
		}
		public object ReadFile(FilePath file, System.Type expectedType, IProgressMonitor monitor)
		{
			XmlTextReader reader = new XmlTextReader(new System.IO.StreamReader(file));
			object result;
			try
			{
				monitor.BeginTask(string.Format(GettextCatalog.GetString("Loading project: {0}"), file), 1);
				reader.MoveToContent();
				XmlDataSerializer ser = new XmlDataSerializer(MD1ProjectService.get_DataContext());
				ser.get_SerializationContext().set_BaseFile(file);
				SolutionEntityItem entry = (SolutionEntityItem)ser.Deserialize(reader, typeof(NUnitAssemblyGroupProject));
				entry.set_FileName(file);
				result = entry;
			}
			catch (System.Exception ex)
			{
				monitor.ReportError(string.Format(GettextCatalog.GetString("Could not load project: {0}"), file), ex);
				throw;
			}
			finally
			{
				monitor.EndTask();
				reader.Close();
			}
			return result;
		}
		public void ConvertToFormat(object obj)
		{
		}
		public System.Collections.Generic.IEnumerable<string> GetCompatibilityWarnings(object obj)
		{
			yield break;
		}
		public bool SupportsFramework(TargetFramework framework)
		{
			return true;
		}
	}
}
