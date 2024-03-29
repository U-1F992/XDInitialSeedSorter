﻿using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using PokemonPRNG.LCG32.GCLCG;
using Narwhal;

namespace PokemonXD
{
    public partial class XDConnecter : IDisposable
    {
        private Setting _setting;
        private GCController _controller;
        private XDCapture _capture;
        private int _delayAfterReset;

        private bool _disposed = false;

        public XDConnecter(Setting? setting) {

            ClearScreen();

            this._setting = VerifySetting(setting);
            this._controller = new GCController(_setting.devices.controller.port, GCResetMethod.BXSt);
            this._capture = new XDCapture(_setting.devices.capture.index, new Size(_setting.devices.capture.width, _setting.devices.capture.height), _setting.devices.capture.crops, _setting.devices.capture.visible, _setting.binaries.tesseractOCR);

            this._delayAfterReset = _setting.devices.controller.delayAfterReset;
        }

        public void ClearScreen()
        {
            Console.WriteLine(@"
                                            /^^        
 ___       _      __                /^^   /^^/^^^^^    
| . \ ___ | |__ _/_/._ _ _  ___ ._ _ /^^ /^^ /^^   /^^ 
|  _// . \| / // ._>| ' ' |/ . \| ' |  /^^   /^^    /^^
|_|  \___/|_\_\\___.|_|_|_|\___/|_|_|/^^ /^^ /^^    /^^
        Gale    of    Darkness      /^^   /^^/^^   /^^ 
  -----===========================/^^========/^^^^^    
");
        }

        /// <summary>
        /// Returns current seed candidates.
        /// Or returns an empty array if there is no applicable candidates.
        /// </summary>
        /// <param name="data1">A QuickBattleData object.</param>
        /// <param name="data2">A QuickBattleData object.</param>
        /// <returns>An array of seed candidates casted to uint32(0x00000000~0xFFFFFFFF)</returns>
        private List<UInt32> GetCandidatesFromQuickBattleData(QuickBattleData data1, QuickBattleData data2)
        {
            string raw;
            string bin = _setting.binaries.xdDatabase;
            try
            {
                using (Process? process = Process.Start(new ProcessStartInfo() {FileName = bin, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true}))
                {
                    if (process == null) throw new Exception("\"" + bin + "\" は開始しませんでした。");
                    process.StandardInput.WriteLine(data1.ToString() + data2.ToString());
                    process.WaitForExit();
                    raw = process.StandardOutput.ReadToEnd();
                }
            }
            catch
            {
                throw new Exception("\"" + bin + "\" は見つかりません。");
            }
            
            // Sample
            // 223455個のseedを読み込みました. ミュウツー, ミュウ, デオキシス, レックウザ, ジラーチ フリーザー, サンダー, ファイヤー, ガルーラ, ラティアス パーティ(こちら あちら) >HP(こちら あちら) >パーティ(こちら あちら) >HP(こちら あちら) >4E193B37 A504D29B 313BBA8B
            string[] array = raw.Split(">");
            raw = array[array.Length - 1];
            raw = Regex.Replace(raw, @"\s+", " ");

            // Remove duplicates
            // https://dobon.net/vb/dotnet/programing/arraydistinct.html
            HashSet<string> hs = new HashSet<string>(raw.Split(" "));
            array = new string[hs.Count];
            hs.CopyTo(array, 0);

            // 「見つかりませんでした.」 or only 1 means there is no applicable candidates.
            if(array.Length < 3) throw new Exception("No candidates were found.");

            List<UInt32> list = new List<UInt32>();
            // Candidates are in index 2, 4, 6...
            for(int i = 2; i < array.Length; i += 2)
            {
                list.Add((UInt32)Convert.ToUInt64(Regex.Replace(array[i], "[^0123456789ABCDEFabcdef]", ""), 16)); // Overflow prevention
            }

            return list;
        }

        /// <summary>
        /// Get current seed from the 「いますぐバトル > さいきょう」 screen.
        /// </summary>
        /// <returns>Current seed</returns>
        public UInt32 GetCurrentSeed()
        {
            QuickBattleData data1;
            QuickBattleData data2;
            List<UInt32> candidates;

            _controller.InvokeSequence(new GCOperation[]
            {
                new GCOperation(GCButton.A, 100, 1000),
                new GCOperation(GCButton.B, 100, 500),
                new GCOperation(GCButton.A, 100, 1000)
            });
            data1 = _capture.GetQuickBattleData();
            Console.WriteLine(data1.ToJson());

            do
            {
                _controller.InvokeSequence(new GCOperation[]
                {
                    new GCOperation(GCButton.B, 100, 500),
                    new GCOperation(GCButton.A, 100, 1000)
                });
                data2 = _capture.GetQuickBattleData();
                Console.WriteLine(data2.ToJson());

                candidates = GetCandidatesFromQuickBattleData(data1, data2);
                
                data1 = data2;

            } while (candidates.Count != 1);
            
            Console.WriteLine("");
            Console.WriteLine("Current seed: " + Convert.ToString(candidates[0], 16));
            Console.WriteLine("");
            return candidates[0];
        }

        /// <summary>
        /// Get current seed after reset.
        /// </summary>
        /// <returns>Current seed</returns>
        public UInt32 GetNextInitialSeed()
        {
            // Lead to the first page
            // 「いますぐバトル > さいきょう」
            _controller.Reset(_delayAfterReset - 15000);
            _controller.InvokeSequence(new GCOperation[]
            {
                new GCOperation(GCButton.A, 100, 19000),
                new GCOperation(GCButton.A, 100, 1800),
                new GCOperation(GCButton.A, 100, 1200),
                new GCOperation(GCButton.dR, 100, 1000),
                new GCOperation(GCButton.A, 100, 1500),
                new GCOperation(GCButton.A, 100, 2500),
                new GCOperation(GCButton.A, 100, 500)
            });

            return GetCurrentSeed();
        }

        public Dictionary<UInt32, TimeSpan> GetWaitingTimes(UInt32 currentSeed)
        {
            Setting.PokemonXD setting = _setting.pokemonXD;

            Dictionary<UInt32, TimeSpan> result = new Dictionary<UInt32, TimeSpan>();
            foreach (UInt32 target in setting.targets)
            {
                long consumption = GCLCGExtension.GetIndex(target, currentSeed);
                TimeSpan ts = new TimeSpan(0, 0, (int)((consumption - setting.leftover) / setting.consumptionPerSec));

                result.Add(target, ts);
            }

            // Show like this:
            //
            // Target    WaitingTime
            // --------  -----------
            // e10e228   12.13:57:23
            // b8262a28  8.17:54:53
            string toShow = "Target    WaitingTime\n";
            toShow       += "--------  -----------\n";
            foreach (var pair in result)
            {
                string seed = Convert.ToString(pair.Key, 16);
                toShow += (seed + new string(' ', 10 - seed.Length) + pair.Value.ToString(@"d\.hh\:mm\:ss") + "\n");
            }
            Console.WriteLine(toShow);

            return result;
        }

        /// <summary>
        /// リセット後の初期seedからtargetsの各seedまでの待機時間で、最も短いものを返します。
        /// </summary>
        /// <returns>TimeSpanオブジェクト</returns>
        public TimeSpan GetShortestWaitingTime()
        {
            Dictionary<UInt32, TimeSpan> result;
            try
            {
                result = GetWaitingTimes(GetNextInitialSeed());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("No candidates were found.");
                return TimeSpan.MaxValue;
            }

            foreach (var pair in result.OrderBy(pair => pair.Value))
            {
                return pair.Value;
            }
            throw new Exception(); // method must return value and this exception is never thrown.
        }

        /// <summary>
        /// いますぐバトル戦闘画面にファイヤーを出し、乱数を大量消費します。
        /// </summary>
        /// <param name="waitingTime">時間は余裕をもって設定する</param>
        public void InvokeRoughConsumption(TimeSpan waitingTime)
        {
            QuickBattleData data = _capture.GetQuickBattleData();

            // Until we find Moltres
            while (data.Party[1] != 2)
            {
                _controller.InvokeSequence(new GCOperation[]
                {
                    new GCOperation(GCButton.B, 100, 500),
                    new GCOperation(GCButton.A, 100, 1000)
                });

                data = _capture.GetQuickBattleData();
            }

            _controller.InvokeOperation(GCOperation.PressA);
            Console.WriteLine("Consuming random numbers with Moltres, ETA: {0}", (DateTime.Now + waitingTime).ToString());
            Thread.Sleep((int)waitingTime.TotalMilliseconds);

            // after consumption
            _controller.InvokeSequence(new GCOperation[]
            {
                GCOperation.PressSt,
                GCOperation.PressdD,
                new GCOperation(GCButton.A, 100, 12000),
                new GCOperation(GCButton.B, 100, 5000)
            });
            Console.WriteLine("Consumption complete.");
            Console.WriteLine("");

            return;
        }

        /// <summary>
        /// this might prevent the program from exiting when over-consume would occur.
        /// </summary>
        /// <param name="currentSeed"></param>
        /// <param name="targetSeed"></param>
        /// <returns></returns>
        public bool IsLeftEnough(UInt32 currentSeed, UInt32 targetSeed)
        {
            var index = targetSeed.GetIndex(currentSeed);
            return _setting.pokemonXD.leftover <= index && index < (4294967295 / 2);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _capture.Dispose();
                    _controller.Dispose();
                }
                _disposed = true;
            }
        }
    }
}