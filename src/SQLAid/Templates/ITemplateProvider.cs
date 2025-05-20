using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SQLAid.Templates
{
    public interface ITemplateProvider
    {
        string FindTemplate(string triggerText);

        Task<string> GetTemplateAsync(string templateName);
    }

    public class TemplateProvider : ITemplateProvider
    {
        private readonly string _templateDirectory;
        private readonly string _customTemplatesPath;

        public TemplateProvider(string extensionDirectory)
        {
            _templateDirectory = Path.Combine(extensionDirectory, "Templates", "Internal");
        }

        public TemplateProvider(string extensionDirectory, string customTemplatesPath) : this(extensionDirectory)
        {
            _customTemplatesPath = customTemplatesPath ?? throw new ArgumentNullException(nameof(customTemplatesPath));
        }

        public string FindTemplate(string triggerText)
        {
            IEnumerable<(string Name, string Content)> templates = GetAllTemplates();
            var template = templates.FirstOrDefault(t => t.Name.Equals(triggerText, StringComparison.CurrentCultureIgnoreCase));
            return template.Content ?? string.Empty;
        }

        private IEnumerable<(string, string)> GetAllTemplates()
        {
            var defaultTemplates = LoadTemplatesFromDirectory(_templateDirectory);
            var customTemplates = LoadTemplatesFromDirectory(_customTemplatesPath);
            return customTemplates.Concat(defaultTemplates);
        }

        private IEnumerable<(string, string)> LoadTemplatesFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return Enumerable.Empty<(string, string)>();

            return Directory.GetFiles(directory, "*.sql", SearchOption.AllDirectories).Select(file => (Path.GetFileNameWithoutExtension(file), File.ReadAllText(file)));
        }

        public Task<string> GetTemplateAsync(string templateName)
        {
            var templatePath = Path.Combine(_templateDirectory, templateName);
            return Task.FromResult(File.ReadAllText(templatePath));
        }
    }
}
