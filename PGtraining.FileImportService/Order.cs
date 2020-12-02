﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace PGtraining.FileImportService
{
    [Table("Orders")]
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string OrderNo { get; set; }
        public string StudyDate { get; set; }
        public string ProcessingType { get; set; }
        public string InspectionTypeCode { get; set; }
        public string InspectionTypeName { get; set; }
        public string PatientId { get; set; }
        public string PatientNameKanji { get; set; }
        public string PatientNameKana { get; set; }
        public string PatientBirth { get; set; }
        public string PatientSex { get; set; }
        public List<string> MenuCodes { get; set; } = new List<string>();
        public List<string> MenuNames { get; set; } = new List<string>();

        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Order()
        {
        }

        public Order(string[] row)
        {
            FileInfo info = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            log4net.Config.XmlConfigurator.Configure(log4net.LogManager.GetRepository(), info);

            List<string> lists = new List<string>();
            lists.AddRange(row);

            this.OrderNo = lists[0];
            this.StudyDate = lists[1];
            this.ProcessingType = lists[2];
            this.InspectionTypeCode = lists[3];
            this.InspectionTypeName = lists[4];
            this.PatientId = lists[5];
            this.PatientNameKanji = lists[6];
            this.PatientNameKana = lists[7];
            this.PatientBirth = lists[8];
            this.PatientSex = lists[9];

            for (var i = 10; i < lists.Count; i++)
            {
                if (i % 2 == 0)
                {
                    this.MenuCodes.Add(lists[i]);
                }
                else
                {
                    this.MenuNames.Add(lists[i]);
                }
            }
        }

        public bool OrderValidation()
        {
            var results = new List<bool>();

            results.Add(this.CheckInspectionTypeCode());
            results.Add(this.CheckInspectionTypeName());
            results.Add(this.CheckMenuCodes());
            results.Add(this.CheckMenuNames());
            results.Add(this.CheckOrderNo());
            results.Add(this.CheckPatientBirth());
            results.Add(this.CheckPatientId());
            results.Add(this.CheckPatientNameKana());
            results.Add(this.CheckPatientNameKanji());
            results.Add(this.CheckPatientSex());
            results.Add(this.CheckProcessingType());
            results.Add(this.CheckStudyDate());

            var result = results.All(x => x = true);
            return result;
        }

        #region 個々のプロパティのバリデーションチェック

        private bool CheckOrderNo()
        {
            if ((CheckString.IsAlphaNumericOnly(this.OrderNo, 8, 8)))
            {
                return true;
            }
            else
            {
                _logger.Error($"オーダ番号:{this.OrderNo} 8文字の半角英数字になっていません。");
                return false;
            }
        }

        private bool CheckStudyDate()
        {
            if ((CheckString.IsDateTime(this.StudyDate, "yyyyMMdd")))
            {
                return true;
            }
            else
            {
                _logger.Error($"検査日付:{this.StudyDate} 有効な日付を表すyyyymmdd形式の半角数字列になっていません。");
                return false;
            }
        }

        private bool CheckProcessingType()
        {
            if ((CheckString.IsMatch(this.ProcessingType, "[1-3]", 1, 1)))
            {
                return true;
            }
            else
            {
                _logger.Error($"処理区分:{this.ProcessingType} 文字の半角数字(1,2,3)のいずれかになっていません。");
                return false;
            }
        }

        private bool CheckInspectionTypeCode()
        {
            if ((CheckString.IsAlphaNumericOnly(this.InspectionTypeCode, 1, 8)))
            {
                return true;
            }
            else
            {
                _logger.Error($"検査種別コード:{this.InspectionTypeCode} 1文字以上、8文字以下の半角英数字列になっていません。");
                return false;
            }
        }

        private bool CheckInspectionTypeName()
        {
            if ((CheckString.IsMatch(this.InspectionTypeName, ".*", 1, 32)))
            {
                return true;
            }
            else
            {
                _logger.Error($"検査種別名称:{this.InspectionTypeName } 1文字以上,32文字以下の任意の文字列になっていません。");
                return false;
            }
        }

        private bool CheckPatientId()
        {
            if ((CheckString.IsAlphaNumericOnly(this.PatientId, 10, 10)))
            {
                return true;
            }
            else
            {
                _logger.Error($"患者ID:{this.PatientId} 10文字の半角英数字列（患者基本属性データの一意な識別子）になっていません。");
                return false;
            }
        }

        private bool CheckPatientNameKanji()
        {
            if (CheckString.IsMatch(this.PatientNameKanji, ".*", 1, 64))
            {
                return true;
            }
            else
            {
                _logger.Error($"患者漢字氏名:{this.PatientNameKanji} 1文字以上,64文字以下の任意の文字列になっていません。");
                return false;
            }
        }

        private bool CheckPatientNameKana()
        {
            if (CheckString.IsKataKana(this.PatientNameKana, 1, 64))
            {
                return true;
            }
            else
            {
                _logger.Error($"患者カナ氏名:{this.PatientNameKana} 1文字以上,64文字以下の任意の文字列になっていません。");
                return false;
            }
        }

        private bool CheckPatientBirth()
        {
            if ((CheckString.IsDateTime(this.PatientBirth, "yyyyMMdd")))
            {
                return true;
            }
            else
            {
                _logger.Error($"患者生年月日:{this.PatientBirth} 有効な日付を表すyyyymmdd形式の半角数字列になっていません。");
                return false;
            }
        }

        private bool CheckPatientSex()
        {
            if ((CheckString.IsMatch(this.PatientSex, "[FMO]", 1, 1)))
            {
                return true;
            }
            else
            {
                _logger.Error($"患者性別:{this.PatientSex} 1文字の半角英字(FMO)になっていません。");
                return false;
            }
        }

        private bool CheckMenuCodes()
        {
            foreach (var menuCode in this.MenuCodes)
            {
                if ((CheckString.IsAlphaNumericOnly(menuCode, 1, 8)))
                {
                    return true;
                }
                else
                {
                    _logger.Error($"撮影項目コード:{menuCode} 1文字以上、8文字以下の半角英数字列になっていません。");
                    return false;
                }
            }

            return false;
        }

        private bool CheckMenuNames()
        {
            foreach (var menuName in this.MenuNames)
            {
                if ((CheckString.IsMatch(menuName, ".*", 1, 32)))
                {
                    return true;
                }
                else
                {
                    _logger.Error($"撮影項目名称:{menuName } 1文字以上,32文字以下の任意の文字列になっていません。");
                    return false;
                }
            }

            return false;
        }

        #endregion 個々のプロパティのバリデーションチェック

        public bool IsRegistered()
        {
            _logger.Info($"データベースに登録済みか確認します「{this.OrderNo}」");

            var result = false;
            try
            {
                var order = DbLib.GetOrder(this.OrderNo);
                result = (order != null);
            }
            catch (Exception ex)
            {
                _logger.Error($"{this.OrderNo}: {ex.ToString()}");
                result = false;
            }

            _logger.Info($"データベースに登録済み？{result}");

            return result;
        }

        public bool InsertOrder()
        {
            _logger.Info($"データベースにinsertします「{this.OrderNo}」");

            try
            {
                var result = DbLib.InsertOrder(this);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"データベースinsertに失敗しました:「{this.OrderNo}」{ex.ToString()}");
                return false;
            }
        }

        public bool DeleteOrder()
        {
            _logger.Info($"データベースからdeleteします「{this.OrderNo}」");

            try
            {
                var result = DbLib.DeleteOrder(this);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"データベースdeleteに失敗しました:「{this.OrderNo}」{ex.ToString()}");
                return false;
            }
        }

        public bool UpdateOrder()
        {
            var delete = this.DeleteOrder();
            var insert = this.InsertOrder();
            return (delete && insert);
        }
    }
}