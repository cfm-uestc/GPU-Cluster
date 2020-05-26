using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public enum VolumeType
    {
        Read,
        ReadWrite
    }
    public enum VolumePath
    {
        Home,
        Data,
        Public
    }
    public class Volume
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid VolumeID { get; set; }
        public string UserID { get; set; }
        private ApplicationUser user;
        public ApplicationUser User
        {
            get { return user; }
            set
            {
                user = value;
                checkVolumeExists();
            }
        }
        private Image image;
        public Image Image
        {
            get { return image; }
            set
            {
                image = value;
                checkVolumeExists();
            }
        }
        [Required]
        [Display(Name="Mounted /home/$USER volume")]
        public string Name { get; set; }
        public VolumeType Type { get; set; }
        public VolumePath Path { get; set; }

        public ICollection<Mounting> Mountings { get; set; }

        private bool checkValid = false;
        private void checkVolumeExists()
        {
            if (checkValid)
            {
                return;
            }
            if (User == null || (Path == VolumePath.Home && Image == null))
            {
                return;
            }
            switch (Path)
            {
                case VolumePath.Data:
                    IOUtils.AddDirectoryIfNotExists(System.IO.Path.Combine(Consts.PublicRootPath, User.Id, "data"));
                    break;
                case VolumePath.Home:
                    var dir = IOUtils.AddDirectoryIfNotExists(System.IO.Path.Combine(Consts.PublicRootPath, User.Id, Image.Tag));
                    IOUtils.Copy(new System.IO.DirectoryInfo("/etc/skel"), dir, false);
                    break;
                case VolumePath.Public:
                    // no necessary to check
                    break;
                default:
                    return;
            }
            checkValid = true;
        }
    }
}