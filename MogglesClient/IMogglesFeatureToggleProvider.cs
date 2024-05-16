using System.Collections.Generic;
using System.Threading.Tasks;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public interface IMogglesFeatureToggleProvider
    {
        Task<List<FeatureToggle>> GetFeatureToggles();
    }
}
