using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Template.Admin
{
    public class AddVersionDto
    {
        public string Version { get; set; } = string.Empty;
        public string? ChangeLog { get; set; }
    }
    public class AddVersionLinkDto
    {
        public string Version { get; set; } = string.Empty;
        public string? ChangeLog { get; set; }
        public string ExternalUrl { get; set; } = string.Empty;
        public string StorageType { get; set; } = "GoogleDrive";  // GoogleDrive / S3 / R2
    }
}
