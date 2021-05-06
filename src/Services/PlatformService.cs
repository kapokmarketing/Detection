// Copyright (c) 2014-2020 Sarin Na Wangkanai, All Rights Reserved.
// Modifications Copyright (c) 2020 Kapok Marketing, Inc.
// The Apache v2. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text.RegularExpressions;

using Wangkanai.Detection.Extensions;
using Wangkanai.Detection.Models;

namespace Wangkanai.Detection.Services
{
    public class PlatformService : IPlatformService
    {
        public Processor Processor { get; }
        public Platform  Name      { get; }
        public Version   Version   { get; }

        public PlatformService(IUserAgentService userAgentService)
        {
            _userAgentService = userAgentService;
        }

        private Processor? _processor;
        private Platform?  _name;
        private Version?   _version;
        public  Processor  Processor => _processor ??= GetProcessor();
        public  Platform   Name      => _name ??= GetPlatform();
        public  Version    Version   => _version ??= GetVersion();

        private Platform GetPlatform()
        {
            var agent = _userAgentService.UserAgent.ToLower();

            if (string.IsNullOrEmpty(agent))
                return Platform.Unknown;

            if (agent.Contains(Platform.Android))
                return Platform.Android;
            if (agent.Contains(Platform.Windows))
                return Platform.Windows;
            if (IsiOS(agent))
                return Platform.iOS;
            if (agent.Contains(Platform.Mac))
                return Platform.Mac;
            if (agent.Contains(Platform.Linux))
                return Platform.Linux;

            return Platform.Others;
        }

        private Version GetVersion()
        {
            var agent = _userAgentService.UserAgent.ToLower();
            var platform = Name;
            return platform switch
            {
                Platform.Unknown => new Version(),
                Platform.Others => new Version(),
                Platform.Windows => ParseOsVersion(agent, "windowsnt"),
                Platform.Android => ParseOsVersion(agent, "android"),
                Platform.Mac => ParseOsVersion(agent, "intelmacosx"),
                Platform.iOS => ParseOsVersion(agent, "cpuiphoneos"),
                Platform.Linux => ParseOsVersion(agent, "rv:"),
                _ => new Version()
            };
        }

        private static readonly Regex _osStartRegex = new Regex(@"\(([^\)]+)\)", RegexOptions.Compiled);

        private static readonly Regex _osParseRegex =
            new Regex(@"(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.\d+)", RegexOptions.Compiled);

        private static Version ParseOsVersion(string agent, string versionPrefix) =>
            _osParseRegex.RegexMatch(
                    (_osStartRegex.RegexMatch(agent)
                         .Captures[0]
                         .Value
                         .RemoveAll(" ", "(", ")")
                         .Split(';')
                         .FirstOrDefault(x => x.StartsWith(versionPrefix, StringComparison.Ordinal)) ??
                     string.Empty)
                    .Replace("_", ".")
                )
                .Value
                .ToVersion();

        private Processor GetProcessor()
        {
            var agent = _userAgentService.UserAgent.ToLower();
            var os = Name;

            if (IsArm(agent, os))
                return Processor.ARM;
            if (IsX64(agent))
                return Processor.x64;
            if (IsX86(agent))
                return Processor.x86;
            if (IsPowerPC(agent, os))
                return Processor.x64;

            return Processor.Others;
        }

        private static bool IsArm(string agent, Platform os)
            => agent.Contains(Processor.ARM)
               || agent.Contains(Platform.Android)
               || os == Platform.iOS;

        private static bool IsPowerPC(string agent, Platform os)
            => os == Platform.Mac
               && !agent.Contains("ppc", StringComparison.Ordinal);

        private static readonly string[] X86DeviceList = {"i86", "i686", Processor.x86.ToStringInvariant()};

        private static readonly IndexTree _x86DeviceIndex = X86DeviceList.BuildIndexTree();

        private static bool IsX86(string agent) => agent.SearchContains(_x86DeviceIndex);

        private static readonly string[] X64DeviceList = {"x86_64", "wow64", Processor.x64.ToStringInvariant()};

        private static readonly IndexTree _x64DeviceIndex = X64DeviceList.BuildIndexTree();
        private static bool IsX64(string agent) => agent.SearchContains(_x64DeviceIndex);

        private static readonly string[] IosDeviceList = {"ipad", "iphone", "ipod", Platform.iOS.ToStringInvariant()};

        private static readonly IndexTree _iosDeviceIndex = IosDeviceList.BuildIndexTree();
        private static bool IsiOS(string agent) => agent.SearchContains(_iosDeviceIndex);
    }
}