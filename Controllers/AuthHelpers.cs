using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace Tracker.Controllers
{
    public class AuthHelpers
    {
        // returns logged in organization
        public Guid GetUserOrganization(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            return new Guid(identity?.FindFirst("OrganizationID")?.Value);
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            return identity?.FindFirst("UserID").Value;
        }

        // should be in appsettings
        private readonly string pythonCmd = "python3";
        private readonly string resetScriptPath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/scripts/demo_seed.py";

        public void DemoSeed(string org_id)
        {
            Console.WriteLine(resetScriptPath);
            RunCmd(pythonCmd, $"{resetScriptPath} {org_id}");
        }

        private void RunCmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }
    }
}
