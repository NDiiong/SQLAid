using System.IO;
using System.Threading.Tasks;

namespace SQLAid.Templates
{
    public interface ITemplateProvider
    {
        Task<string> GetTemplateAsync(string templateName);
    }

    public class FileTemplateProvider : ITemplateProvider
    {
        private readonly string _templateDirectory;

        public FileTemplateProvider(string extensionDirectory)
        {
            _templateDirectory = Path.Combine(extensionDirectory, "Templates", "Internal");
        }

        public Task<string> GetTemplateAsync(string templateName)
        {
            var templatePath = Path.Combine(_templateDirectory, templateName);
            return Task.FromResult(File.ReadAllText(templatePath));
        }
    }
}