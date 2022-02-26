using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Fastenshtein;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace PokemonXD
{
    public class XDCapture : GCCapture
    {
        private string _tesseract;
        private Dictionary<string, Dictionary<string, int>> _crops;
        
        /// <summary>
        /// キャプチャデバイスから画像を取得します。
        /// </summary>
        public XDCapture(int index, System.Drawing.Size size, Dictionary<string, Dictionary<string, int>> crops, bool visible, string tesseract) : base(index, size, visible)
        {
            this._tesseract = tesseract;
            this._crops = crops;
            base._windowName = "XDCapture";
        }

        public QuickBattleData GetQuickBattleData()
        {
            var frame = GetImage();
            string now = DateTime.Now.ToString("yyyyMMddhhmmss");
#if DEBUG
            BitmapConverter.ToBitmap(frame).Save(now + ".png");
#endif

            Dictionary<string, int> pair = new Dictionary<string, int>();
            Parallel.ForEach(_crops, range =>
            {
                string fn = ".tmp-" + range.Key + ".png";

                using (Mat img = new Mat(frame, new OpenCvSharp.Rect(range.Value["x"],range.Value["y"],range.Value["width"],range.Value["height"])))
                {
                    Cv2.CvtColor(img, img, ColorConversionCodes.RGB2GRAY);
                    Cv2.BitwiseNot(img, img);
                    Cv2.Threshold(img, img, 127, 255, ThresholdTypes.Binary);
                    Cv2.Resize(img, img, new Size(), 2, 2);
                    BitmapConverter.ToBitmap(img).Save(fn);
                }

                string lang = range.Key.Contains("hp_") ? "xdn" : "xdj";
                string raw;

                try
                {
                    using (Process? process = Process.Start(new ProcessStartInfo() {FileName = _tesseract, Arguments = fn+" stdout -l "+lang+" --psm 8", UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8, RedirectStandardOutput = true, RedirectStandardError = true}))
                    {
                        if (process == null) throw new Exception("\"" + _tesseract + "\" は開始しませんでした。");
                        process.WaitForExit();
                        raw = Regex.Replace(process.StandardOutput.ReadToEnd(), @"\s", "");
                    }
                }
                catch
                {
                    throw new Exception("\"" + _tesseract + "\" は見つかりません。");
                }
                finally
                {
#if !DEBUG
                    System.IO.File.Delete(fn);
#endif
                }

                if (range.Key.Contains("hp_"))
                {
                    if (raw.Length != 3) throw new Exception("HPの値が不正です。");
                    lock (pair)
                    {
                        pair.Add(range.Key, Convert.ToInt32(raw));
                    }
                }
                else
                {
                    lock (pair)
                    {
                        pair.Add(range.Key, GetClosestIndex(raw, (range.Key == "player") ? new string[] {"ミュウツー", "ミュウ", "デオキシス", "レックウザ", "ジラーチ"} : new string[] {"フリーザー", "サンダー", "ファイヤー", "ガルーラ", "ラティアス"}));
                    }
                }
            });

            return new QuickBattleData(new int[] {pair["player"], pair["com"]}, new int[] {pair["hp_1"], pair["hp_2"], pair["hp_3"], pair["hp_4"]});
        }

        private int GetClosestIndex(string value, string[] targets)
        {
            List<int> list = new List<int>();
            List<int> sort = new List<int>();
            for (int i = 0; i < targets.Length; i++)
            {
                int dist = Levenshtein.Distance(value, targets[i]);
                list.Add(dist);
                sort.Add(dist);
            }
            sort.Sort();
            int min = sort[0];

            for (int i = 0; i < list.ToArray().Length; i++) if (list[i] == min)
            {
                return i;
            }
            throw new Exception();
        }
    }
}