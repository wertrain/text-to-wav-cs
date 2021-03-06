﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using CommandLineParser;
using System.IO;

namespace TextToWav
{
    class Options
    {
        // スピーチの速度
        [Option('r', "rate", Required = false, HelpText = "Speech rate")]
        public int Rate { get; set; }
        // 入力ファイルパス
        [Option('i', "input", Required = false, HelpText = "Input file path")]
        public string Input { get; set; }
        // 出力ファイルパス
        [Option('o', "output", Required = false, HelpText = "Output file path")]
        public string Output { get; set; }
        // 読み上げテキスト
        [Option('t', "text", Required = false, HelpText = "Speech Text")]
        public string Text { get; set; }
        // 話者
        [Option('s', "speaker", Required = false, HelpText = "Speaker")]
        public string Voice { get; set; }
        // ボリューム
        [Option('v', "volume", Required = false, HelpText = "Speech Volume")]
        public int Volume { get; set; }
        // 無視する単語
        [Option('g', "ignore", Required = false, HelpText = "Ignore Words file path")]
        public string IgnoreWords { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Parse<Options>(args);
            if (result.Tag == ParserResultType.Parsed)
            {
                // パース成功時
                var parsed = result;

                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    string text = string.Empty;
                    if (System.IO.File.Exists(parsed.Value.Input))
                    {
                        synth.SetOutputToDefaultAudioDevice();

                        using (StreamReader sr = new StreamReader(parsed.Value.Input))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(parsed.Value.Text))
                    {
                        text = parsed.Value.Text;
                    }
                    else
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(parsed.Value.IgnoreWords))
                    {
                        var ignoreWords = new List<string>();
                        using (StreamReader sr = new StreamReader(parsed.Value.IgnoreWords))
                        {
                            string rawWords = sr.ReadToEnd();
                            ignoreWords.AddRange(
                                rawWords.Split(new string[] { "\r\n", ",", "\t" }, StringSplitOptions.RemoveEmptyEntries)
                            );
                        }

                        foreach (var word in ignoreWords)
                        {
                            text = text.Replace(word, "");
                        }
                    }

                    if (string.IsNullOrWhiteSpace(parsed.Value.Output))
                    {
                        synth.SetOutputToDefaultAudioDevice();
                    }
                    else
                    {
                        synth.SetOutputToWaveFile(parsed.Value.Output);
                    }

                    string voice = "Microsoft Zira Desktop";
                    if (!string.IsNullOrWhiteSpace(parsed.Value.Voice))
                    {
                        voice = parsed.Value.Voice;
                    }

                    if (parsed.Value.Volume > 0)
                    {
                        synth.Volume = parsed.Value.Volume;
                    }

                    synth.SelectVoice(voice);
                    synth.Rate = parsed.Value.Rate;
                    synth.Speak(text);
                }
            }
            else
            {
                Parser.ShowHelp<Option>();
            }
        }
    }
}
