using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnicalTests.Agent
{
    public class ProjectEditor
    {        
        private string projectPath;
        private Project project;

        private string createdFolder;
        private string createdFile;

        public ProjectEditor(string csprojPath)
        {
            this.projectPath = Path.GetDirectoryName(csprojPath);
            this.project = new Project(csprojPath);            
        }

        public void IncludeFileInProject(string folderName, string code, string destinationFileName)
        {
            CopyFileToProjectFolder(folderName, code, destinationFileName);            
            IncludeFileInCsproj();
        }        

        private void CopyFileToProjectFolder(string folderName, string code, string fileName)
        {
            var folderToBeCreated = Path.Combine(projectPath, folderName);

            RemoveFolderIfItExists(folderToBeCreated);
            createdFolder = CreateFolder(folderToBeCreated);            
            createdFile = CreateFile(code, fileName, createdFolder);            
        }

        private void IncludeFileInCsproj()
        {
            RemoveFileFromCsprojIfItExists();
            string relativePath = GetCreatedFolderRelativePath();

            project.AddItem("Compile", relativePath);
            project.Save();
        }

        private string GetCreatedFolderRelativePath()
        {
            var splitedPath = createdFile.Split('\\');

            var fileName = splitedPath.Last();
            var lastFolder = splitedPath.ElementAt(splitedPath.Count() - 2);
            var lastFolderAndFile = Path.Combine(lastFolder, fileName);

            return lastFolderAndFile;
        }

        private void RemoveFileFromCsprojIfItExists()
        {
            string relativePath = GetCreatedFolderRelativePath();

            var item = project.Items.FirstOrDefault(x => x.EvaluatedInclude == relativePath);

            if (item != null)
            {
                project.RemoveItem(item);
                project.Save();
            }
        }

        private string CreateFile(string code, string fileName, string folder)
        {
            var destinationFile = Path.Combine(folder, fileName);
            File.WriteAllText(destinationFile, code);

            return destinationFile;
        }

        private string CreateFolder(string folderToBeCreated)
        {
            var createdDirectory = Directory.CreateDirectory(folderToBeCreated);
            return createdDirectory.FullName;
        }

        private void RemoveFolderIfItExists(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }        

        public void RollbackEditions()
        {                        
            RemoveFileFromCsprojIfItExists();
            RemoveFolderIfItExists(createdFolder);
        }        
    }
}
