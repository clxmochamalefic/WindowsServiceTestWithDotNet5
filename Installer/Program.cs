using System;
using System.Xml.Linq;

namespace Installer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                WixSharp.File ui = new WixSharp.File(
                    @"cli\WindowsServiceTestClient.exe",
                    new WixSharp.FileShortcut("WindowsServiceTestClient", @"%ProgramMenu%\cocoalix") { Advertise = true }
                );
                WixSharp.File service = new WixSharp.File(
                    @"service\WindowsServiceTest.exe"
                );
                WixSharp.Files uiDepends = new WixSharp.Files(
                    @"cli\*.*"
                );
                WixSharp.Files serviceDepends = new WixSharp.Files(
                    @"service\*.*"
                );

                var dir = new WixSharp.Dir(new WixSharp.Id("InstallDir"),
                    @"%ProgramFiles%\cocoalix",
                    // インストーラーにインクルードするファイル
                    ui,
                    // インストーラーにインクルードするファイル
                    service,
                    uiDepends,
                    serviceDepends
                );

                var project = new WixSharp.ManagedProject("ウィンドウズサービステスト", dir);

                // 日本語をインストーラ及びWiXで仕様するためのおまじない
                project.Codepage = "932";
                project.Language = "ja-JP";

                // アップグレードコードを指定 (変更厳禁)
                project.GUID = new Guid("abbb7cf9-19fa-45f2-babc-a35312741772");

                // インストーラーのファイル名
                project.OutFileName = "ウィンドウズサービステストインストーラ";

                service.ServiceInstaller = new WixSharp.ServiceInstaller
                {
                    Account = @"NT Authority\System",
                    PermissionEx = new WixSharp.PermissionEx()
                    {
                        User = @"Administrator",
                        ServicePauseContinue = false,
                        ServiceStart = true,
                        ServiceStop = true,
                    },
                    Name = "WindowsServiceTestService",
                    Description = "ウィンドウズサービステストのサービスです",
                    StartOn = WixSharp.SvcEvent.Install,
                    StopOn = WixSharp.SvcEvent.InstallUninstall_Wait,
                    RemoveOn = WixSharp.SvcEvent.Uninstall_Wait,
                    DelayedAutoStart = true,
                    FirstFailureActionType = WixSharp.FailureActionType.restart,
                    SecondFailureActionType = WixSharp.FailureActionType.restart,
                    ThirdFailureActionType = WixSharp.FailureActionType.none,
                    PreShutdownDelay = 1000 * 60 * 3,
                    ServiceSid = WixSharp.ServiceSid.none,
                };

                // インストーラで表示するウィンドウ群の指定
                project.ManagedUI = new WixSharp.ManagedUI();
                project.ManagedUI.InstallDialogs.Add(WixSharp.Forms.Dialogs.Welcome)
                                                .Add(WixSharp.Forms.Dialogs.Licence)
                                                .Add(WixSharp.Forms.Dialogs.Progress)
                                                .Add(WixSharp.Forms.Dialogs.Exit);

                project.LicenceFile = @"Eula.rtf";

                project.PreserveTempFiles = true;
                project.WixSourceGenerated += _WixSourceGenerated;

                project.BuildMsi();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void _WixSourceGenerated(XDocument document)
        {
        }

    }
}
