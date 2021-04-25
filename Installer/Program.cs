using System;
using System.Xml.Linq;
using WixSharp;
using WixSharp.Bootstrapper;

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

                project.Platform = Platform.x64;

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

                // インストール時権限昇格
                project.InstallPrivileges = InstallPrivileges.elevated;
                project.InstallScope = InstallScope.perMachine;

                project.PreserveTempFiles = true;

                var projectMsi = project.BuildMsi();

                var bootstrapper = new Bundle(
                    "ウィンドウズサービステスト_バンドルインストーラ",
                    new ExePackage()
                    {
                        Id = "DotNet5DesktopRuntime",
                        Name = "dotnet5-windowsdesktop-runtime-5.0-win-x64.exe",
                        Vital = true,
                        Permanent = false,
                        DownloadUrl = @"https://download.visualstudio.microsoft.com/download/pr/7a5d15ae-0487-428d-8262-2824279ccc00/6a10ce9e632bce818ce6698d9e9faf39/windowsdesktop-runtime-5.0.4-win-x64.exe",
                        InstallCommand = "/install /quiet",
                        RepairCommand = "/repair /quiet",
                        UninstallCommand = "/uninstall /quiet",
                        LogPathVariable = "dotnet5desktopruntime.log",
                        Compressed = true,

                        // RemotePayloadは以下のコマンドで取得可能
                        // heat payload <バンドルしたいexeのバイナリのパス> -out .\remote.xml
                        RemotePayloads = new[]
                        {
                            new RemotePayload()
                            {
                                ProductName = "Microsoft Windows Desktop Runtime - 5.0.4 (x64)",
                                Description = "Microsoft Windows Desktop Runtime - 5.0.4 (x64)",
                                Hash="33FBCDB6B6F052FCC26B4EF850B81ED5F2C10B02",
                                Size = 54790696,
                                Version = "5.0.4.29817".ToRawVersion(),
                                CertificatePublicKey = "3756E9BBF4461DCD0AA68E0D1FCFFA9CEA47AC18",
                                CertificateThumbprint = "2485A7AFA98E178CB8F30C9838346B514AEA4769"
                            }
                        }
                    },
                    new MsiPackage(projectMsi)
                );

                // ランタイムバンドルインストーラのバージョン
                bootstrapper.Version = new Version("1.0.0.0");

                // ランタイムバンドルインストーラのアップグレードコード (変更厳禁)
                bootstrapper.UpgradeCode = new Guid("bf3b1aeb-12c5-4401-ad23-6a49f905bd55");

                // ランタイムバンドルインストーラのアプリケーションスタイルの定義
                bootstrapper.Application = new LicenseBootstrapperApplication();
                bootstrapper.Application.LicensePath = @".\Eula.rtf";

                bootstrapper.Application.LocalizationFile = "thm.wxl";

                // インストール時のOption非表示
                bootstrapper.Application.SuppressOptionsUI = true;
                // アンインストール時の修復を非表示
                bootstrapper.Application.SuppressRepair = true;

                // 一次領域を使用するか
                bootstrapper.PreserveTempFiles = true;
                // Wixの必須パラメータの定義？は行わない
                bootstrapper.SuppressWixMbaPrereqVars = false;

                // インストーラ名の定義
                bootstrapper.OutFileName = "ウィンドウズサービステスト_バンドルインストーラ";

                // ランタイムバンドルインストーラの作成
                bootstrapper.Build();
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
