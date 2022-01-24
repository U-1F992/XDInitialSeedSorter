﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Fastenshtein;
using OpenCvSharp;
using OpenCvSharp.Extensions;

public class XDCapture : GCCapture
{
    private string _tesseract;
    private Dictionary<string, Setting.Rect> _crops;
    
    /// <summary>
    /// キャプチャデバイスから画像を取得します。
    /// </summary>
    /// <param name="setting">Setting.Devices.Captureオブジェクト</param>
    public XDCapture(Setting setting) : base(setting.devices.capture.index, new System.Drawing.Size(setting.devices.capture.width, setting.devices.capture.height), setting.devices.capture.showImage)
    {
        this._tesseract = setting.binaries.tesseractOCR;
        this._crops = setting.crops;
    }

    public QuickBattleData GetQuickBattleData()
    {
        var frame = GetImage();
        // BitmapConverter.ToBitmap(frame).Save(DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");

        Dictionary<string, int> pair = new Dictionary<string, int>();
        Parallel.ForEach(_crops, range =>
        {
            using (Mat img = new Mat(frame, new Rect(range.Value.x, range.Value.y, range.Value.width, range.Value.height)))
            {
                Cv2.CvtColor(img, img, ColorConversionCodes.RGB2GRAY);
                Cv2.BitwiseNot(img, img);
                Cv2.Threshold(img, img, 127, 255, ThresholdTypes.Binary);
                Cv2.Resize(img, img, new Size(), 2, 2);
                BitmapConverter.ToBitmap(img).Save(".tmp"+range.Key+".png");
            }

            string lang;
            if (range.Key.Contains("hp_"))
            {
                lang = "xdn";
            }
            else
            {
                lang = "xdj";
            }

            string raw;
            try
            {
                using (Process? process = Process.Start(new ProcessStartInfo() {FileName = _tesseract, Arguments = ".tmp"+range.Key+".png stdout -l "+lang+" --psm 8", UseShellExecute = false, StandardOutputEncoding = Encoding.UTF8, RedirectStandardOutput = true, RedirectStandardError = true}))
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
                System.IO.File.Delete(".tmp"+range.Key+".png");
            }

            if (range.Key.Contains("hp_"))
            {
                if (raw.Length != 3) throw new Exception("HPの値が不正です。");
                pair.Add(range.Key, Convert.ToInt32(raw));
            }
            else
            {
                string[] samples;
                if (range.Key == "player")
                {
                    samples = new string[] {"ミュウツー", "ミュウ", "デオキシス", "レックウザ", "ジラーチ"};
                }
                else
                {
                    samples = new string[] {"フリーザー", "サンダー", "ファイヤー", "ガルーラ", "ラティアス"};
                }
                pair.Add(range.Key, GetClosestIndex(raw, samples));
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