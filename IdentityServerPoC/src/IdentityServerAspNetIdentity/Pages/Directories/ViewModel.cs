using Directory = IdentityServerAspNetIdentity.Models.Directory;

namespace IdentityServerAspNetIdentity.Pages.Directories;

public class ViewModel
{
    public IEnumerable<DetailedDirectory> Directories;

    public class DetailedDirectory
    {
        public string Name { get; set; }
        public bool IsHomeDirectory { get; set; }
    }
}

