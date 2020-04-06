using Microsoft.Build.Construction;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.DotNet.AzureDevOps.Build
{
    public class GenerateAzureDevOpsBuildManifest : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string AzureDevOpsCollectionUri { get; set; }
        [Required]
        public string AzureDevOpsProject { get; set; }
        [Required]
        public int AzureDevOpsBuildId { get; set; }
        public string ManifestPath { get; set; }
        public ITaskItem[] ItemsToSign { get; set; }
        public ITaskItem[] StrongNameSignInfo { get; set; }
        public ITaskItem[] FileSignInfo { get; set; }
        public ITaskItem[] FileExtensionSignInfo { get; set; }
        public override bool Execute()
        {
            ProjectRootElement project = ProjectRootElement.Create();
            var propertyGroup = project.CreatePropertyGroupElement();
            project.AppendChild(propertyGroup);
            propertyGroup.AddProperty("AzureDevOpsCollectionUri", AzureDevOpsCollectionUri);
            propertyGroup.AddProperty("AzureDevOpsProject", AzureDevOpsProject);
            propertyGroup.AddProperty("AzureDevOpsBuildId", AzureDevOpsBuildId.ToString());
            var itemGroup = project.CreateItemGroupElement();
            project.AppendChild(itemGroup);
            if (ItemsToSign != null)
            {
                foreach (var itemToSign in ItemsToSign)
                {
                    var filename = itemToSign.ItemSpec.Replace('\\', '/');
                    {
                        var metadata = itemToSign.CloneCustomMetadata() as Dictionary<string, string>;
                        var fileExtensionSignInfo = FileExtensionSignInfo.FirstOrDefault(f => Path.GetExtension(itemToSign.ItemSpec).Equals(f.ItemSpec, StringComparison.InvariantCultureIgnoreCase));
                        var fileSignInfo = FileSignInfo.FirstOrDefault(f => Path.GetFileName(itemToSign.ItemSpec).Equals(f.ItemSpec, StringComparison.InvariantCultureIgnoreCase));

                        itemGroup.AddItem("ItemsToSign", Path.GetFileName(itemToSign.ItemSpec), metadata);
                    }
                }
            }
            foreach (var signInfo in StrongNameSignInfo)
            {
                itemGroup.AddItem("StrongNameSignInfo", Path.GetFileName(signInfo.ItemSpec), signInfo.CloneCustomMetadata() as Dictionary<string, string>);
            }
            foreach (var signInfo in FileSignInfo)
            {
                itemGroup.AddItem("FileSignInfo", signInfo.ItemSpec, signInfo.CloneCustomMetadata() as Dictionary<string, string>);
            }
            foreach (var signInfo in FileExtensionSignInfo)
            {
                itemGroup.AddItem("FileExtensionSignInfo", signInfo.ItemSpec, signInfo.CloneCustomMetadata() as Dictionary<string, string>);
            }
            project.Save(ManifestPath);
            return true;
        }
    }
}
