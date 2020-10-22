using System.Collections.Generic;
using System.Reflection;

namespace MogglesClient.Messaging.EnvironmentDetector
{
    public interface IAssemblyProvider
    {
        List<Assembly> GetCurrentDomainAssemblies();
    }
}