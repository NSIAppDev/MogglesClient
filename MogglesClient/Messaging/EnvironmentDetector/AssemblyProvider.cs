using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MogglesClient.Messaging.EnvironmentDetector
{
    public class AssemblyProvider : IAssemblyProvider
    {
        public List<Assembly> GetCurrentDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache).ToList();
        }
    }
}
