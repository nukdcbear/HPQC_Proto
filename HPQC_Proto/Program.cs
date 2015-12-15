using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SACLIENTLib;
using CommandLine;
using CommandLine.Text;
using DotNet.Config;

namespace HPQC_Proto
{
    class Program
    {
        static void Main(string[] args)
        {

            SAapi saconnect = new SAapi();
            String SourceDomain = "POC";
            String SourceProject = "BaseProjectwithTasktopFields";
            String NewDomain = "POC";
            String NewProject = "RemoteCreate";
            String ServerURL = "";
            int DB_Type = (int)ENUM_SA_DB_TYPE_OPTIONS.SA_MSSQL_DB_TYPE;
            String DBServer = "DEVSQL94";
            String DBAdmin = "hpalm";
            String DBAdminPWD = "hpalm2015";
            int ProjCopyOpts = (int)SA_COPY_PROJECT_OPTIONS.SA_COPY_PROJECT_CUSTOMIZATION;
            int ProjCreateOpts = (int)ENUM_SA_PROJECT_CREATION_OPTIONS.SA_ACTIVATE_NEW_PROJECT + (int)ENUM_SA_PROJECT_CREATION_OPTIONS.SA_CREATE_VERSION_CONTROL_DB;

            Options cmdlnOptions = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, cmdlnOptions))
            {
                // consume Options instance properties
                if (cmdlnOptions.Verbose)
                {
                    Console.WriteLine(cmdlnOptions.HPQCenv);
                    Console.WriteLine(cmdlnOptions.almUser);
                }
                else
                    Console.WriteLine("working ...");
            }
            else
            {
                // Display the default usage information
                Console.WriteLine(cmdlnOptions.GetUsage());
            }

            switch (cmdlnOptions.HPQCenv.ToLower())
            {
                case "dev":
                    ServerURL = AppSettings.Retrieve("HPQC_Proto.properties")["dev-serverurl"];
                    break;
                case "test":
                    ServerURL = AppSettings.Retrieve("HPQC_Proto.properties")["test-serverurl"];
                    break;
                default:
                    Console.WriteLine("Environment not supported: " + cmdlnOptions.HPQCenv);
                    Environment.Exit(1);
                    break;
            }
            

            // Simple command line argument processing; [0] = Server URL, [1] = User ID, [2] = User password
            //if (args.Length < 3)
            //{
            //    Console.WriteLine("Please enter required arguments; Server URL, User ID and User password!");
            //    Environment.Exit(1);
            //}

            String almUser = cmdlnOptions.almUser;
            String almUserPassword = cmdlnOptions.almUserPasswd;

            try
            {
                saconnect.Login(ServerURL, almUser, almUserPassword);
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            saconnect.DisconnectProject(SourceDomain, SourceProject);
            saconnect.DeactivateProject(SourceDomain, SourceProject);

            try
            {
                saconnect.CreateProjectCopy(NewDomain, NewProject, DB_Type, SourceDomain, SourceProject, DBServer, DBAdmin, DBAdminPWD, "TableSpace", "tempTableSpace", 0, 0, ProjCopyOpts, ProjCreateOpts);
                saconnect.ActivateProject(SourceDomain, SourceProject);

                Console.WriteLine("Project Created");

                saconnect.LinkProjects(SourceDomain, SourceProject, NewDomain, NewProject, "template");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            saconnect.Logout();
            Console.WriteLine("Logged Out!");
        }

    }

    // Commandline options
    class Options
    {
        [Option('e', "env", Required = true, HelpText = "HP QC environment.")]
        public string HPQCenv { get; set; }

        [Option('u', "user", Required = true, HelpText = "HP QC user ID.")]
        public string almUser { get; set; }

        [Option('p', "password", Required = true, HelpText = "HP QC user password.")]
        public string almUserPasswd { get; set; }

        [Option('v', null, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild
            var usage = new StringBuilder();
            usage.AppendLine("HP QC Project Creator v0.9");
            usage.AppendLine("Read user manual for usage instructions...");
            return usage.ToString();
        }
    }
}
