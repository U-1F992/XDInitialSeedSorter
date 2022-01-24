using System.Diagnostics;
using System.Text;

namespace PokemonXD
{
    public partial class XDConnecter : IDisposable
    {
        private Setting VerifySetting(Setting? setting)
        {
            if (
                setting == null ||
                setting.pokemonXD == null ||
                setting.pokemonXD.targets == null ||
                setting.binaries == null ||
                setting.binaries.tesseractOCR == null ||
                setting.binaries.xdDatabase == null ||
                setting.devices == null ||
                setting.devices.controller == null ||
                setting.devices.controller.port == null ||
                setting.devices.capture == null ||
                setting.devices.capture.crops == null
            ) throw new Exception("設定の形式に誤りがあります。");

            // Pathが通っているか確認する
            // TesseractOCR
            string raw;
            string bin = setting.binaries.tesseractOCR;
            try
            {
                using (Process? process = Process.Start(new ProcessStartInfo() {FileName = bin, Arguments = "--list-langs", UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8, RedirectStandardOutput = true, RedirectStandardError = true}))
                {
                    if (process == null) throw new Exception("\"" + bin + "\" は開始しませんでした。");
                    process.WaitForExit();
                    raw = process.StandardOutput.ReadToEnd();
                }
            }
            catch
            {
                throw new Exception("\"" + bin + "\" は見つかりません。");
            }
            if (!raw.Contains("xdn")) throw new Exception("言語データ \"xdn\" がインストールされていません。");
            if (!raw.Contains("xdj")) throw new Exception("言語データ \"xdj\" がインストールされていません。");

            // XDDatabase
            bin = setting.binaries.xdDatabase;
            try
            {
                using (Process? process = Process.Start(new ProcessStartInfo() {FileName = bin, Arguments = "--list-langs", UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true}))
                {
                    if (process == null) throw new Exception("\"" + bin + "\" は開始しませんでした。");
                    process.Kill();
                }
            }
            catch
            {
                throw new Exception("\"" + bin + "\" は見つかりません。");
            }

            return setting;
        }
    }
}