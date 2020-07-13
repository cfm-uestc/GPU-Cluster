using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GPUCluster.WebService.Models;
using Microsoft.AspNetCore.Authorization;
using GPUCluster.Shared.K8s;
using GPUCluster.WebService.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using GPUCluster.Shared.Models.Instance;
using Microsoft.EntityFrameworkCore;

namespace GPUCluster.WebService.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IK8sInvoker _k8sInvoker;
        private readonly IdentityDataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IdentityDataContext context, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, IK8sInvoker k8sInvoker)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _k8sInvoker = k8sInvoker;
        }

        public IActionResult Index()
        {
            return View(new IndexViewModel());
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Deploy()
        {
            var container = _context.Container.Include(x => x.Image).Include(x => x.Mountings).Include(x => x.User).ThenInclude(u => u.LinuxUser).ToList();
            var mounting = _context.Mounting.Include(x => x.Container).Include(x => x.User).Include(x => x.Volume).ToList();
            return await _k8sInvoker.DeployAsync(container[0]) ? (IActionResult)Ok() : (IActionResult)BadRequest();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
