// <copyright file="OptionsConfigSource.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

using Mono.Options;

namespace TestFtpServer
{
    public class OptionsConfigSource : IConfigurationSource
    {
        private readonly string[] _args;

        public OptionsConfigSource(string[] args)
        {
            _args = args;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var authentication = new List<string>();
            var values = new Dictionary<string, string>();
            var optionSet = new CommandSet("ftpserver")
            {
                "usage: ftpserver [OPTIONS] <COMMAND> [COMMAND-OPTIONS]",
                { "?|help", "Show help", v => { /* Handled internally by the Options class. */ } },
                "Authentication",
                { "authentication=", "Sets the authentication (custom, pam, anonymous)", v =>
                    {
                        switch (v)
                        {
                            case "default":
                                authentication.Add("anonymous");
                                break;
                            case "custom":
                            case "anonymous":
                            case "pam":
                                authentication.Add(v);
                                break;
                            default:
                                throw new ApplicationException("Invalid authentication module");
                        }
                    }
                },
                "PAM authentication workarounds",
                { "no-pam-account-management", "Disable the PAM account management", v => values["pam:noAccountManagement"] = v != null ? "true" : "false" },
                "Directory layout (system-io, unix))",
                { "l|layout=", "Directory layout (single-root (default), root-per-user, pam-home, pam-home-chroot)", v => {
                        switch (v)
                        {
                            case "default":
                            case "single-root":
                                values["layout"] = "single-root";
                                break;
                            case "root-per-user":
                            case "pam-home":
                            case "pam-home-chroot":
                                values["layout"] = v;
                                break;
                            default:
                                throw new ApplicationException("Invalid authentication module");
                        }
                    }
                },
                "Server",
                { "a|address=", "Sets the IP address or host name", v => values["server:address"] = v },
                { "p|port=", "Sets the listen port", v => values["server:port"] = Convert.ToInt32(v).ToString(CultureInfo.InvariantCulture) },
                { "d|data-port", "Bind to the data port", v => values["server:useFtpDataPort"] = v != null ? "true" : "false" },
                { "s|pasv=", "Sets the range for PASV ports, specify as FIRST:LAST", v => values["server:pasv:range"] = v },
                { "promiscuous", "Allows promiscuous PASV", v => values["server:pasv:promiscuous"] = v != null ? "true" : "false" },
                "FTPS",
                { "c|certificate=", "Set the SSL certificate", v => values["ftps:certificate"] = v },
                { "k|private-key=", "Set the private key file for the SSL certificate", v => values["ftps:privateKey"] = v },
                { "P|password=", "Password for the SSL certificate", v => values["ftps:password"] = v },
                { "i|implicit", "Use implicit FTPS", v => values["ftps:implicit"] = XmlConvert.ToBoolean(v.ToLowerInvariant()) ? "true" : "false" },
                "Backends",
                new Command("system-io", "Use the System.IO file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver system-io [ROOT-DIRECTORY]",
                    },
                    Run = a => ConfigureSystemIo(a.ToArray(), values),
                },
                new Command("unix", "Use the Unix file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver unix",
                    },
                    Run = a => ConfigureUnix(a.ToArray(), values),
                },
                new Command("in-memory", "Use the in-memory file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver in-memory [OPTIONS]",
                        { "keep-anonymous", "Keep anonymous in-memory file systems", v => values["inMemory:keepAnonymous"] = v != null ? "true" : "false" }
                    },
                    Run = a => ConfigureInMemory(values),
                },
                new CommandSet("google-drive")
                {
                    { "b|background|background-upload", "Use background upload", v => values["googleDrive:backgroundUpload"] = v != null ? "true" : "false" },
                    new Command("user", "Use a users Google Drive as file system")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive user <CLIENT-SECRETS-FILE> <USERNAME>",
                            { "r|refresh", "Refresh the access token", v => values["googleDrive:user:refreshToken"] = v != null ? "true" : "false" },
                        },
                        Run = a => ConfigureGoogleDriveUser(a.ToArray(), values),
                    },
                    new Command("service", "Use a users Google Drive with a service account")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive service <SERVICE-CREDENTIAL-FILE>",
                        },
                        Run = a => ConfigureGoogleDriveService(a.ToArray(), values),
                    },
                },
            };

            if (_args.Length != 0)
            {
                var exitCode = Execute(optionSet);

                if (exitCode != 0 || optionSet.showHelp)
                {
                    Environment.Exit(exitCode);
                }
            }

            if (authentication.Count != 0)
            {
                values["authentication"] = string.Join(",", authentication.Distinct());
            }

            var source = new MemoryConfigurationSource()
            {
                InitialData = values,
            };

            return new MemoryConfigurationProvider(source);
        }

        private int Execute(CommandSet optionSet)
        {
            optionSet.showHelp = false;
            if (optionSet.help == null)
            {
                optionSet.help = new HelpCommand();
                optionSet.AddCommand(optionSet.help);
            }

            void SetHelp(string v) => optionSet.showHelp = v != null;
            if (!optionSet.Options.Contains("help"))
            {
                optionSet.Options.Add("help", "", SetHelp, hidden: true);
            }

            if (!optionSet.Options.Contains("?"))
            {
                optionSet.Options.Add("?", "", SetHelp, hidden: true);
            }

            var extra = optionSet.Options.Parse(_args);
            if (extra.Count == 0)
            {
                if (optionSet.showHelp)
                {
                    return optionSet.help.Invoke(extra);
                }

                return 0;
            }

            var command = optionSet.GetCommand(extra);
            if (command == null)
            {
                optionSet.help.WriteUnknownCommand(extra[0]);
                return 1;
            }

            if (optionSet.showHelp)
            {
                if (command.Options == null || command.Options.Contains("help"))
                {
                    extra.Add("--help");
                    return command.Invoke(extra);
                }

                command.Options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            return command.Invoke(extra);
        }

        private void ConfigureInMemory(
            Dictionary<string, string> values)
        {
            values["backend"] = "in-memory";
        }

        private void ConfigureGoogleDriveService(
            string[] args,
            Dictionary<string, string> values)
        {
            if (args.Length != 1)
            {
                throw new Exception("This command requires one argument: <SERVICE-CREDENTIAL-FILE>");
            }

            values["backend"] = "google-drive:service";
            values["googleDrive:service:credentialFile"] = args[0];
        }

        private void ConfigureGoogleDriveUser(
            string[] args,
            Dictionary<string, string> values)
        {
            if (args.Length != 2)
            {
                throw new Exception("This command requires two arguments: <CLIENT-SECRETS-FILE> <USERNAME>");
            }

            values["backend"] = "google-drive:user";
            values["googleDrive:user:clientSecrets"] = args[0];
            values["googleDrive:user:userName"] = args[1];
        }

        private void ConfigureUnix(
            string[] args,
            IDictionary<string, string> values)
        {
            values["backend"] = "unix";

            if (args.Length != 0)
            {
                values["unix:root"] = args[0];
            }
        }

        private void ConfigureSystemIo(
            string[] args,
            IDictionary<string, string> values)
        {
            values["backend"] = "system-io";

            if (args.Length != 0)
            {
                values["systemIo:root"] = args[0];
            }
        }
    }
}
