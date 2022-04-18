using Directory = IdentityServerAspNetIdentity.Models.Directory;

namespace IdentityServerAspNetIdentity.Pages.Directories;

public class ViewModel
{
    public readonly IEnumerable<DetailedDirectory> Directories;

    public ViewModel(IEnumerable<Directory> directories, Guid? homeDirectoryId)
    {
        Directories = directories.Select(directory => new DetailedDirectory()
        {
            Name = directory.Name,
            IsHomeDirectory = directory.Id == homeDirectoryId
        });
    }

    public class DetailedDirectory
    {
        public string Name { get; set; }
        public bool IsHomeDirectory { get; set; }
    }
}

