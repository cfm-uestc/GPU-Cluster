using System;
using GPUCluster.Shared;
using GPUCluster.Shared.Models.Workload;

namespace GPUCluster.WebService.Models
{
    public class ContainerViewModel
    {
        Container _container;
        Image _selectedImage;
        Volume _homeVolume;
        public ContainerViewModel(Container container)
        {
            _container = container;
        }
    }
}
