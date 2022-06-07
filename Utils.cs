using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LAB_7
{
    public static class Utils {
        internal static bool CheckInput(string input) {
            input = input.Trim();
            foreach (string v in input.Split()) {
                if (!Regex.IsMatch(v, "(-?\\d*\\.|,\\d+)|(-?\\d+(\\.|,)?\\d*)")) {
                    return false;
                }
            }

            return true;
        }

        internal static string RandomInput() {
            string res = "";
            Random R = new();

            int N = R.Next(10, 30);
            for (int i = 0; i < N; i++) {
                // Add delimiter
                if (res != "") res += " ";
                // Generate number
                double v = Math.Round(R.NextDouble() * R.Next(1, 20 + 1), R.Next(2, 5 + 1)) * (R.Next(0, 2) == 1 ? 1 : -1);
                res += v.ToString();
            }

            return res;
        }

        //public static void WriteBinary()
        public static IEnumerable<double> ReadBinFile(FileStream inputFileStream) {
            using BinaryReader binReader = new(inputFileStream);
            inputFileStream.Seek(0, SeekOrigin.Begin);
            double v;
            while (true) {
                try {
                    v = binReader.ReadDouble();
                }
                catch (EndOfStreamException) {
                    break;
                }
                yield return v;
            }
        }

        public static void WriteBinFile(FileStream outputFileStream, string output) {
            using BinaryWriter binWriter = new(outputFileStream);
            outputFileStream.Seek(0, SeekOrigin.Begin);
            foreach (string s in output.Split()) {
                binWriter.Write(double.Parse(s));
            }
        }
    }
}