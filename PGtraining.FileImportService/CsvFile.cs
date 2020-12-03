﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PGtraining.FileImportService
{
    public class CsvFile
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Import(string path)
        {
            FileInfo info = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            log4net.Config.XmlConfigurator.Configure(log4net.LogManager.GetRepository(), info);

            _logger.Info($"Import Start {path}【読込開始】");

            var result = true;

            StreamReader sr = new StreamReader(@path, System.Text.Encoding.GetEncoding("shift_jis"));
            {
                var row = 0;
                while (!sr.EndOfStream)
                {
                    row++;

                    string line = sr.ReadLine();

                    //ヘッダーを飛ばす
                    if (row == 1)
                    {
                        continue;
                    }

                    string[] values = line.Split(',');

                    //項目数を確認
                    if (values.Length < 12)
                    {
                        _logger.Error($"検査の項目数は12項目以上必要です。項目数が{ values.Length }です。");
                        result = false;
                        continue;
                    }
                    if (values.Length % 2 != 0)
                    {
                        _logger.Error($"検査の項目数は偶数必要です。項目数が{ values.Length }です。");
                        result = false;
                        continue;
                    }

                    _logger.Info($"{row}行目読込：{ line }");

                    //ダブルクォーテーションがあるか確認
                    var doubleQuotesError = false;
                    for (var i = 0; i < values.Length; i++)
                    {
                        var check = this.CheckDoubleQuotes(values[i]);

                        if (check)
                        {
                            values[i] = values[i].Substring(1, values[i].Length - 2);
                        }
                        else
                        {
                            _logger.Error($"ダブルクォーテーションがありません。{ values[i] }です。");
                            result = false;
                            doubleQuotesError = true;
                        }
                    }

                    if (doubleQuotesError)
                    {
                        result = false;
                        continue;
                    }

                    //検査の読込
                    _logger.Info($"{ string.Join(",", values) }");

                    var order = new Order(values);
                    if (order.OrderValidation())
                    {
                        _logger.Info($"{row}行目{row - 1}検査目：読込問題なし");
                    }
                    else
                    {
                        _logger.Error($"{row}行目{row - 1}検査目：読込エラーあり");
                        result = false;
                        continue;
                    }

                    //登録済みなら、上書き、未登録なら追加
                    if (order.IsRegistered())
                    {
                        _logger.Info($"上書きします");
                        var update = order.UpdateOrder();

                        if (update == false)
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        _logger.Info($"登録します");
                        var insert = order.InsertOrder();

                        if (insert == false)
                        {
                            result = false;
                        }
                    }
                }
            }
            sr.Close();

            _logger.Info($"Import End {path}【読込終了】");

            return result;
        }

        private bool CheckDoubleQuotes(string value)
        {
            return Regex.IsMatch(value, "^\".*\"");
        }
    }
}