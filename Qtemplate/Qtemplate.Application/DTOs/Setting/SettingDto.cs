using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Setting
{
    public class UpdateSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Group { get; set; }
    }
    public class SettingItemDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string Group { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class CreateSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string Group { get; set; } = "General";
        public string? Description { get; set; }
    }
}
