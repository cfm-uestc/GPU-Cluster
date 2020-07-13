using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GPUCluster.Shared;
using GPUCluster.Shared.Models.Instance;
using GPUCluster.Shared.Models.Workload;
using GPUCluster.WebService.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace GPUCluster.WebService.Models
{
    public class ContainerViewModel
    {
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public Guid ImageID { get; set; }
        [Required]
        public Guid VolumeID { get; set; }
        public Container Container { get; set; }
        public Image Image { get; set; }
        public Volume Volume { get; set; }
        internal async Task<Container> ValidateContainerAsync(IdentityDataContext context, ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(ContainerName))
            {
                throw new ArgumentNullException(nameof(ContainerName));
            }
            Container = new Container
            {
                Name = ContainerName
            };
            Volume = await context.Volume.FirstAsync(v => v.VolumeID == VolumeID);
            Image = await context.Image.FirstAsync(v => v.VolumeID == ImageID);
            Container.Image = Image;
            Container.ImageID = Image.VolumeID;
            Volume publicVolume = await context.Volume.FirstAsync(x => x.UserID == user.Id && x.Path == VolumePath.Public);
            Volume dataVolume = await context.Volume.FirstAsync(x => x.UserID == user.Id && x.Path == VolumePath.Data);
            Mounting publicMounting = new Mounting
            {
                Volume = publicVolume,
                Container = this.Container,
                User = user,
                UserID = user.Id
            };
            Mounting dataMounting = new Mounting
            {
                Volume = dataVolume,
                Container = this.Container,
                User = user,
                UserID = user.Id
            };
            Mounting homeMounting = new Mounting
            {
                Volume = this.Volume,
                Container = this.Container,
                User = user,
                UserID = user.Id
            };
            Container.Mountings = new Mounting[] { publicMounting, dataMounting, homeMounting };
            Container.User = user;
            Container.UserID = user.Id;
            return Container;
        }
    }
}
